using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Entities;
using OrderApi.Entities.Dto;
using OrderApi.Services;
using System.Net;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly RabbitMqService _rabbitMqService;
        private readonly MenuService _menuService;

        public OrdersController(RabbitMqService rabbitMqService, MenuService menuService, OrderDbContext context)
        {
            _rabbitMqService = rabbitMqService;
            _menuService = menuService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CriarPedido([FromHeader] Guid clientId,[FromBody] PedidoDto dto)
        {
            var itens = new List<ItemPedido>();
            decimal total = 0;
            var pedidoId = Guid.NewGuid();

            foreach (var itemDto in dto.Itens)
            {
                var produto = await _menuService.GetProdutoByIdAsync(itemDto.ProdutoId);
                if (produto == null)
                    return BadRequest($"Produto {itemDto.ProdutoId} não encontrado no cardápio.");

                if (!produto.Disponivel)
                    return BadRequest($"Produto '{produto.Nome}' não está disponível no momento.");

                if (produto.Quantidade < itemDto.Quantidade)
                    return BadRequest($"Estoque insuficiente para '{produto.Nome}'. Disponível: {produto.Quantidade}, Solicitado: {itemDto.Quantidade}");

                var itemPedido = new ItemPedido
                {
                    Id = Guid.NewGuid(),
                    PedidoId = pedidoId,
                    ProdutoId = itemDto.ProdutoId, 
                    Produto = produto.Nome,
                    Quantidade = itemDto.Quantidade,
                    PrecoUnitario = produto.Preco
                };

                itens.Add(itemPedido);
                total += itemPedido.Quantidade * itemPedido.PrecoUnitario;

                var novoEstoque = produto.Quantidade - itemDto.Quantidade;
                var estoqueAtualizado = await _menuService.AtualizarEstoqueAsync(itemDto.ProdutoId, novoEstoque);
                
                if (!estoqueAtualizado)
                {
                    return StatusCode(500, $"Erro ao atualizar estoque do produto '{produto.Nome}'");
                }
            }

            var pedido = new Pedido
            {
                Id = pedidoId,
                Cliente = clientId,
                Itens = itens,
                Total = total,
                Status = StatusPedido.Pendente,
                Justificativa= string.Empty,
                FormaEntrega = dto.FormaEntrega,
                DataCriacao = DateTime.UtcNow
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            _rabbitMqService.PublishOrder(pedido);

            return Ok(new { pedido.Id, pedido.Total, pedido.Status });
        }

        [HttpGet("byStatus")]
        public async Task<IActionResult> GetByStatus([FromHeader] StatusPedido status)
        {
            var pendentes = await _context.Pedidos.Where(p => p.Status == status).ToListAsync();
            var todosPendentes = new List<Pedido>();
            foreach(var item in pendentes)
            {
                var items = await _context.ItemPedido.Where(p => p.PedidoId == item.Id).ToListAsync();
                todosPendentes.Add(new Pedido
                {
                    Id = item.Id,
                    Cliente = item.Cliente,
                    Itens = items,
                    Total = item.Total,
                    Status = item.Status,
                    Justificativa = item.Justificativa,
                    FormaEntrega = item.FormaEntrega,
                    DataCriacao = item.DataCriacao
                });
            }
            return Ok(todosPendentes);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoverPedido(Guid id, [FromBody] string justificativa)
        {
            if (string.IsNullOrWhiteSpace(justificativa))
            {
                return BadRequest("Justificativa é obrigatória para cancelar o pedido.");
            }

            var pedidoRabbit = _rabbitMqService.ConsumirPedidoPorId(id);
            
            if (pedidoRabbit == null)
            {
                return BadRequest("Pedido não pode ser cancelado. Não está na fila de pendentes (pode já ter sido aceito ou processado).");
            }

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound("Pedido não encontrado no banco de dados.");

            var itens = await _context.ItemPedido.Where(i => i.PedidoId == id).ToListAsync();
            foreach (var item in itens)
            {
                var produtoAtual = await _menuService.GetProdutoByIdAsync(item.ProdutoId);
                if (produtoAtual != null)
                {
                    var novoEstoque = produtoAtual.Quantidade + item.Quantidade;
                    await _menuService.AtualizarEstoqueAsync(item.ProdutoId, novoEstoque);
                }
            }

            pedido.Status = StatusPedido.Cancelado;
            pedido.Justificativa = justificativa;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Pedido removido da fila e cancelado com sucesso.", justificativa });
        }

        [HttpPost("{id}/aceitar")]
        public async Task<IActionResult> AceitarPedido(Guid id)
        {
            var pedidoRabbit = _rabbitMqService.ConsumirPedidoPorId(id);
            if (pedidoRabbit == null)
                return NotFound("Pedido não encontrado na fila de pendentes.");

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.Status = StatusPedido.Aceito;
            await _context.SaveChangesAsync();

            _rabbitMqService.PublishAcceptedOrder(pedido);

            return Ok(new { message = "Pedido aceito e enviado para preparo." });
        }

        [HttpPost("{id}/recusar")]
        public async Task<IActionResult> RecusarPedido(Guid id, [FromBody] string justificativa)
        {
            if (string.IsNullOrWhiteSpace(justificativa))
            {
                return BadRequest("Justificativa é obrigatória para recusar o pedido.");
            }

            var pedidoRabbit = _rabbitMqService.ConsumirPedidoPorId(id);
            if (pedidoRabbit == null)
                return NotFound("Pedido não encontrado na fila de pendentes.");

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            var itens = await _context.ItemPedido.Where(i => i.PedidoId == id).ToListAsync();
            foreach (var item in itens)
            {
                var produtoAtual = await _menuService.GetProdutoByIdAsync(item.ProdutoId);
                if (produtoAtual != null)
                {
                    var novoEstoque = produtoAtual.Quantidade + item.Quantidade;
                    await _menuService.AtualizarEstoqueAsync(item.ProdutoId, novoEstoque);
                }
            }

            pedido.Status = StatusPedido.Recusado;
            pedido.Justificativa = justificativa;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Pedido recusado e removido da fila.", justificativa });
        }

        [HttpPut("{id}/finalizar")]
        public async Task<IActionResult> FinalizarPedido(Guid id)
        {
            var pedidoRabbit = _rabbitMqService.ConsumirPedidoAceitoPorId(id);
            if (pedidoRabbit == null)
                return NotFound("Pedido não encontrado na fila de aceitos.");

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.Status = StatusPedido.Finalizado;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Pedido finalizado." });
        }

    }

}
