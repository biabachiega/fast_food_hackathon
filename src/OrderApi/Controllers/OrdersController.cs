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

                var itemPedido = new ItemPedido
                {
                    Id = Guid.NewGuid(),
                    PedidoId = pedidoId,
                    Produto = produto.Nome,
                    Quantidade = itemDto.Quantidade,
                    PrecoUnitario = produto.Preco
                };

                itens.Add(itemPedido);
                total += itemPedido.Quantidade * itemPedido.PrecoUnitario;
            }

            var pedido = new Pedido
            {
                Id = pedidoId,
                Cliente = clientId,
                Itens = itens,
                Total = total,
                Status = StatusPedido.EmMontagem,
                Justificativa= string.Empty,
                FormaEntrega = dto.FormaEntrega,
                DataCriacao = DateTime.UtcNow
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

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

        private static readonly Dictionary<StatusPedidoCliente, StatusPedido> StatusMap = new()
        {
            { StatusPedidoCliente.Pendente, StatusPedido.Pendente },
            { StatusPedidoCliente.Cancelado, StatusPedido.Cancelado }
        };

        [HttpPut("cliente/enviarPedidoOuCancelar")]
        public async Task<IActionResult> AceitarPedido([FromHeader]Guid id,[FromBody]StatusPedidoCliente status)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            if (!StatusMap.TryGetValue(status, out var statusPedido))
                return BadRequest("Status inválido.");

            if (pedido.Status == StatusPedido.Pendente || pedido.Status == StatusPedido.EmMontagem)
            {
                pedido.Status = statusPedido;
                await _context.SaveChangesAsync();

                if (status == StatusPedidoCliente.Cancelado)
                {
                    return Ok(new { message = "Pedido cancelado com sucesso" });
                }
                return Ok(new { message = "Pedido enviado com sucesso" });

            }
            return BadRequest("Status do Pedido não pode ser alterado");
        }


        [HttpPost("{id}/aceitar")]
        public async Task<IActionResult> AceitarPedido(Guid id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.Status = StatusPedido.Aceito;
            await _context.SaveChangesAsync();

            // Agora sim, publique na fila do RabbitMQ
            _rabbitMqService.PublishOrder(pedido);

            return Ok(new { message = "Pedido aceito e enviado para preparo." });
        }

        [HttpPost("{id}/recusar")]
        public async Task<IActionResult> RecusarPedido(Guid id, [FromBody] string justificativa)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.Status = StatusPedido.Recusado;
            pedido.Justificativa = justificativa;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Pedido recusado." });
        }
    }

}
