using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Entities;
using OrderApi.Entities.Dto;
using OrderApi.Services;

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
                Status = StatusPedido.Pendente,
                Justificativa= string.Empty,
                FormaEntrega = dto.FormaEntrega,
                DataCriacao = DateTime.UtcNow
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return Ok(new { pedido.Id, pedido.Total, pedido.Status });
        }

        [HttpGet("pendentes")]
        public async Task<IActionResult> GetPendentes()
        {
            var pendentes = await _context.Pedidos.Where(p => p.Status == StatusPedido.Pendente).ToListAsync();
            return Ok(pendentes);
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
