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
            var mensagensTemporarias = new List<(ulong DeliveryTag, string Json)>();
            Pedido? pedidoEncontrado = null;

            try
            {
                // Processa todas as mensagens da fila até encontrar o pedido ou a fila estar vazia
                while (true)
                {
                    result = _channel.BasicGet(queue: queueName, autoAck: false);

                    if (result == null)
                        break; // Nenhuma mensagem na fila

                    var body = result.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    var pedido = JsonSerializer.Deserialize<Pedido>(json);

                    if (pedido != null && pedido.Id == id)
                    {
                        // Encontrou o pedido! Remove da fila
                        _channel.BasicAck(result.DeliveryTag, false);
                        pedidoEncontrado = pedido;
                    }
                    else
                    {
                        // Guarda a mensagem temporariamente para re-enfileirar depois
                        mensagensTemporarias.Add((result.DeliveryTag, json));
                    }
                }

                // Re-enfileira todas as mensagens que não foram removidas, mantendo a ordem original
                foreach (var (deliveryTag, json) in mensagensTemporarias)
                {
                    // Primeiro faz ACK da mensagem original
                    _channel.BasicAck(deliveryTag, false);
                    
                    // Depois re-publica na fila para manter a ordem
                    var bodyBytes = Encoding.UTF8.GetBytes(json);
                    var properties = _channel.CreateBasicProperties();
                    properties.Persistent = true;
                    _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: bodyBytes);
                }

                return pedidoEncontrado;
            }
            catch (Exception)
            {
                // Em caso de erro, re-enfileira todas as mensagens usando NACK
                foreach (var (deliveryTag, _) in mensagensTemporarias)
                {
                    _channel.BasicNack(deliveryTag, false, true);
                }
                throw;
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
