using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessor.Services
{
    public class PedidoProcessorService
    {
        private readonly ILogger<PedidoProcessorService> _logger;

        public PedidoProcessorService(ILogger<PedidoProcessorService> logger)
        {
            _logger = logger;
        }

        public Task ProcessarPedidoAsync(string pedidoJson)
        {
            // Aqui você pode deserializar o JSON e salvar/processar no banco
            _logger.LogInformation("Processando pedido: {pedido}", pedidoJson);
            return Task.CompletedTask;
        }
    }
}
