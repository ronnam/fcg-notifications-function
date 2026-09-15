using Fgc.MessageContracts.Events;
using Fgc.Notifications.Application.Handlers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Fgc.Notifications.Function;

public class PaymentProcessedTrigger
{
    private readonly PaymentProcessedHandler _handler;
    private readonly ILogger<PaymentProcessedTrigger> _logger;

    public PaymentProcessedTrigger(PaymentProcessedHandler handler, ILogger<PaymentProcessedTrigger> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    [Function("PaymentProcessedTrigger")]
    public async Task Run(
        [RabbitMQTrigger("notifications-payment-processed-queue", ConnectionStringSetting = "RabbitMQConnection")] string envelope)
    {
        _logger.LogInformation("Mensagem recebida na fila notifications-payment-processed-queue.");
        var @event = MassTransitEnvelopeParser.Parse<PaymentProcessedEvent>(envelope);
        await _handler.Handle(@event);
    }
}