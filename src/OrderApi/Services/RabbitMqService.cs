using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Entities;

namespace OrderApi.Services
{

    public class RabbitMqService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqService()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                UserName = "fasttech",
                Password = "fasttech123"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declara as duas filas para o fluxo de pedidos
            _channel.QueueDeclare(queue: "pending_orders", durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: "accepted_orders", durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public void PublishOrder(object order)
        {
            var json = JsonSerializer.Serialize(order);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            // Publica na fila "pending_orders" (pedidos confirmados pelo cliente)
            _channel.BasicPublish(exchange: "", routingKey: "pending_orders", basicProperties: properties, body: body);
        }

        public void PublishAcceptedOrder(object order)
        {
            var json = JsonSerializer.Serialize(order);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            // Publica na fila "accepted_orders" (pedidos aceitos pela cozinha)
            _channel.BasicPublish(exchange: "", routingKey: "accepted_orders", basicProperties: properties, body: body);
        }

        public Pedido? ConsumirPedidoPorId(Guid id)
        {
            return ConsumirPedidoPorIdDaFila(id, "pending_orders");
        }

        public Pedido? ConsumirPedidoAceitoPorId(Guid id)
        {
            return ConsumirPedidoPorIdDaFila(id, "accepted_orders");
        }

        private Pedido? ConsumirPedidoPorIdDaFila(Guid id, string queueName)
        {
            BasicGetResult result;
            int maxTries = 100; // Limite para evitar loop infinito

            for (int i = 0; i < maxTries; i++)
            {
                result = _channel.BasicGet(queue: queueName, autoAck: false);

                if (result == null)
                    return null; // Nenhuma mensagem na fila

                var body = result.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var pedido = JsonSerializer.Deserialize<Pedido>(json);

                if (pedido != null && pedido.Id == id)
                {
                    _channel.BasicAck(result.DeliveryTag, false);
                    return pedido;
                }
                else
                {
                    // Re-enfileira a mensagem para não perder
                    _channel.BasicNack(result.DeliveryTag, false, true);
                }
            }

            return null; // Pedido não encontrado na fila
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
