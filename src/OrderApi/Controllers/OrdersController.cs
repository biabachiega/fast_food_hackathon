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

        public OrdersController(RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        [HttpPost]
        public async Task<IActionResult> CriarPedido([FromHeader] Guid clientId,[FromBody] PedidoDto dto)
        {
            // Calcule o total
            decimal total = dto.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);

            var pedido = new Pedido
            {
                Id = clientId,
                Cliente = dto.Cliente,
                Itens = dto.Itens.Select(i => new ItemPedido
                {
                    Id = clientId,
                    Produto = i.Produto,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario
                }).ToList(),
                Total = total,
                Status = StatusPedido.Pendente,
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
