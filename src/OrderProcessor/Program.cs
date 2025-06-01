using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderProcessor.Consumers;
using OrderProcessor.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Registra o hosted service que escuta o RabbitMQ
        services.AddHostedService<PedidoConsumer>();

        // Serviço responsável por processar o conteúdo do pedido
        services.AddScoped<PedidoProcessorService>();
    })
    .Build();

await host.RunAsync();
