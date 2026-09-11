using Fgc.MessageContracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Fgc.Notifications.Application.Consumers;

public class UserCreatedEventConsumer(ILogger<UserCreatedEventConsumer> logger)
    : IConsumer<UserCreatedEvent>
{
    public Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var @event = context.Message;
        logger.LogInformation(
            "📧 [WELCOME EMAIL] Para: {email} | Assunto: Bem-vindo à FCG, {userName}! | Corpo: Olá {userName}, sua conta foi criada com sucesso! Aproveite nosso catálogo de jogos educativos.",
            @event.Email,
            @event.Name,
            @event.Name
        );
        return Task.CompletedTask;
    }
}