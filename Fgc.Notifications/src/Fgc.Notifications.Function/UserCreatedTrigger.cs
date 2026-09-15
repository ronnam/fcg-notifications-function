using Fgc.MessageContracts.Events;
using Fgc.Notifications.Application.Handlers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Fgc.Notifications.Function;

public class UserCreatedTrigger
{
    private readonly UserCreatedHandler _handler;
    private readonly ILogger<UserCreatedTrigger> _logger;

    public UserCreatedTrigger(UserCreatedHandler handler, ILogger<UserCreatedTrigger> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    [Function("UserCreatedTrigger")]
    public async Task Run(
        [RabbitMQTrigger("notifications-user-created-queue", ConnectionStringSetting = "RabbitMQConnection")] string envelope)
    {
        _logger.LogInformation("Mensagem recebida na fila notifications-user-created-queue.");
        var @event = MassTransitEnvelopeParser.Parse<UserCreatedEvent>(envelope);
        await _handler.Handle(@event);
    }
}