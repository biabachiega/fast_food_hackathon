using Microsoft.AspNetCore.Mvc;
using OrderApi.Entities.Dto;
using OrderApi.Services;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly RabbitMqService _rabbitMqService;

        public OrdersController(RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] CreateOrderDto dto)
        {
            // Validação básica
            if (dto == null || dto.Itens == null || !dto.Itens.Any())
                return BadRequest("Pedido inválido.");

            // Publica o pedido na fila RabbitMQ
            _rabbitMqService.PublishOrder(dto);

            return Accepted("Pedido recebido e será processado.");
        }
    }
}
