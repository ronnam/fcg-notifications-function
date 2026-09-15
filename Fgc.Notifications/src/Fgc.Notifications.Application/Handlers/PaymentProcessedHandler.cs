using Fgc.MessageContracts.Events;
using Microsoft.Extensions.Logging;

namespace Fgc.Notifications.Application.Handlers;

public class PaymentProcessedHandler(ILogger<PaymentProcessedHandler> logger)
{
    public Task Handle(PaymentProcessedEvent @event)
    {
        if (@event.Status == "Approved")
        {
            logger.LogInformation(
                "📧 [PURCHASE CONFIRMATION] Para: {userId} | Assunto: Compra confirmada! | Corpo: Sua compra de {price:C2} foi aprovada. GameId: {gameId}",
                @event.UserId, @event.Price, @event.GameId);
        }
        else if (@event.Status == "Rejected")
        {
            logger.LogWarning(
                "⚠️ [PAYMENT REJECTED] Payment {orderedId} foi rejeitado.",
                @event.OrderedId);
        }
        return Task.CompletedTask;
    }
}