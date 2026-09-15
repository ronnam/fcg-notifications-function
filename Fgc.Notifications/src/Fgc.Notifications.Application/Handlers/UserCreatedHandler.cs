using Fgc.MessageContracts.Events;
using Microsoft.Extensions.Logging;

namespace Fgc.Notifications.Application.Handlers;

public class UserCreatedHandler(ILogger<UserCreatedHandler> logger)
{
    public Task Handle(UserCreatedEvent @event)
    {
        logger.LogInformation(
            "📧 [WELCOME EMAIL] Para: {email} | Assunto: Bem-vindo à FCG, {userName}! | Corpo: Olá {userName}, sua conta foi criada com sucesso! Aproveite nosso catálogo de jogos educativos.",
            @event.Email, @event.Name, @event.Name);
        return Task.CompletedTask;
    }
}