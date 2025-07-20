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

            _channel.QueueDeclare(queue: "pending_orders", durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: "accepted_orders", durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public void PublishOrder(object order)
        {
            var json = JsonSerializer.Serialize(order);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: "", routingKey: "pending_orders", basicProperties: properties, body: body);
        }

        public void PublishAcceptedOrder(object order)
        {
            var json = JsonSerializer.Serialize(order);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

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
            var mensagensTemporarias = new List<(ulong DeliveryTag, string Json)>();
            Pedido? pedidoEncontrado = null;

            try
            {
                while (true)
                {
                    result = _channel.BasicGet(queue: queueName, autoAck: false);

                    if (result == null)
                        break;

                    var body = result.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    var pedido = JsonSerializer.Deserialize<Pedido>(json);

                    if (pedido != null && pedido.Id == id)
                    {
                        _channel.BasicAck(result.DeliveryTag, false);
                        pedidoEncontrado = pedido;
                    }
                    else
                    {
                        mensagensTemporarias.Add((result.DeliveryTag, json));
                    }
                }

                foreach (var (deliveryTag, json) in mensagensTemporarias)
                {
                    _channel.BasicAck(deliveryTag, false);
                    
                    var bodyBytes = Encoding.UTF8.GetBytes(json);
                    var properties = _channel.CreateBasicProperties();
                    properties.Persistent = true;
                    _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: bodyBytes);
                }

                return pedidoEncontrado;
            }
            catch (Exception)
            {
                foreach (var (deliveryTag, _) in mensagensTemporarias)
                {
                    _channel.BasicNack(deliveryTag, false, true);
                }
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
