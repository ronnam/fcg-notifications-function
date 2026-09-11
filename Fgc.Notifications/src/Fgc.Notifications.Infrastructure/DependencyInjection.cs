using Fgc.Notifications.Application.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fgc.Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<UserCreatedEventConsumer>();
            bus.AddConsumer<PaymentProcessedEventConsumer>();

            bus.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMq:Host"] ?? "localhost";
                var virtualHost = configuration["MassTransit:VirtualHost"] ?? "/";
                var username = configuration["MassTransit:Username"] ?? "admin";
                var password = configuration["MassTransit:Password"] ?? "admin";

                var userCreatedQueue = configuration["RabbitMq:UserCreatedQueueName"]
                    ?? "notifications-user-created-queue";
                var paymentProcessedQueue = configuration["RabbitMq:PaymentProcessedQueueName"]
                    ?? "notifications-payment-processed-queue";

                cfg.Host(host, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ReceiveEndpoint(userCreatedQueue, e =>
                    e.ConfigureConsumer<UserCreatedEventConsumer>(context));

                cfg.ReceiveEndpoint(paymentProcessedQueue, e =>
                    e.ConfigureConsumer<PaymentProcessedEventConsumer>(context));
            });
        });

        return services;
    }
}