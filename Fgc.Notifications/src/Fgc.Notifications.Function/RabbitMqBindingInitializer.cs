using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Fgc.Notifications.Function;

public class RabbitMqBindingInitializer : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqBindingInitializer> _logger;

    public RabbitMqBindingInitializer(IConfiguration configuration, ILogger<RabbitMqBindingInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var host = _configuration["RabbitMq:Host"] ?? "localhost";
        var username = _configuration["MassTransit:Username"] ?? "admin";
        var password = _configuration["MassTransit:Password"] ?? "admin";
        var virtualHost = _configuration["MassTransit:VirtualHost"] ?? "/";

        var userCreatedQueue = _configuration["RabbitMq:UserCreatedQueueName"] ?? "notifications-user-created-queue";
        var paymentProcessedQueue = _configuration["RabbitMq:PaymentProcessedQueueName"] ?? "notifications-payment-processed-queue";

        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = username,
            Password = password,
            VirtualHost = virtualHost
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        Bind(channel, "Fgc.MessageContracts.Events:UserCreatedEvent", userCreatedQueue);
        Bind(channel, "Fgc.MessageContracts.Events:PaymentProcessedEvent", paymentProcessedQueue);

        _logger.LogInformation("RabbitMQ: filas e bindings criados com sucesso.");
        return Task.CompletedTask;
    }

    private static void Bind(IModel channel, string exchange, string queue)
    {
        channel.ExchangeDeclare(exchange, ExchangeType.Fanout, durable: true);
        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue, exchange, routingKey: "");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}