using Fgc.MessageContracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Fgc.Notifications.Application.Consumers;

public class PaymentProcessedEventConsumer(ILogger<PaymentProcessedEventConsumer> logger)
    : IConsumer<PaymentProcessedEvent>
{
    public Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var @event = context.Message;
        if (@event.Status == "Approved")
        {
            logger.LogInformation(
                "📧 [PURCHASE CONFIRMATION] Para: {userId} | Assunto: Compra confirmada! | Corpo: Sua compra de {price:C2} foi aprovada. GameId: {gameId}",
                @event.UserId,
                @event.Price,
                @event.GameId
            );
        }
        else if (@event.Status == "Rejected")
        {
            logger.LogWarning(
                "⚠️ [PAYMENT REJECTED] Payment {orderedId} foi rejeitado.",
                @event.OrderedId
            );
        }
        return Task.CompletedTask;
    }
}