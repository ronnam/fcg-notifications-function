using System;
using Fgc.MessageContracts.Events;
using Fgc.Notifications.Application.Handlers;
using Fgc.Notifications.UnitTests.Helpers;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Fgc.Notifications.UnitTests.Handlers;

public class PaymentProcessedHandlerTests
{
    private readonly TestLogger<PaymentProcessedHandler> _logger = new();
    private readonly PaymentProcessedHandler _handler;

    public PaymentProcessedHandlerTests()
    {
        _handler = new PaymentProcessedHandler(_logger);
    }

    [Fact]
    public void Handle_QuandoStatusApproved_LogaConfirmacaoDeCompra()
    {
        var @event = new PaymentProcessedEvent(
            OrderedId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            GameId: Guid.NewGuid(),
            Price: 29.90m,
            Status: "Approved",
            ProcessedAt: DateTime.UtcNow);

        _handler.Handle(@event).GetAwaiter().GetResult();

        var log = Assert.Single(_logger.Logs);
        Assert.Equal(LogLevel.Information, log.Level);
        Assert.Contains("PURCHASE CONFIRMATION", log.Message);
        Assert.Contains("Compra confirmada", log.Message);
    }

    [Fact]
    public void Handle_QuandoStatusRejected_LogaAvisoDeRejeicao()
    {
        var @event = new PaymentProcessedEvent(
            OrderedId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            GameId: Guid.NewGuid(),
            Price: 29.90m,
            Status: "Rejected",
            ProcessedAt: DateTime.UtcNow);

        _handler.Handle(@event).GetAwaiter().GetResult();

        var log = Assert.Single(_logger.Logs);
        Assert.Equal(LogLevel.Warning, log.Level);
        Assert.Contains("PAYMENT REJECTED", log.Message);
    }

    [Fact]
    public void Handle_QuandoStatusDesconhecido_NaoLogaNada()
    {
        var @event = new PaymentProcessedEvent(
            OrderedId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            GameId: Guid.NewGuid(),
            Price: 29.90m,
            Status: "Pending",
            ProcessedAt: DateTime.UtcNow);

        _handler.Handle(@event).GetAwaiter().GetResult();

        Assert.Empty(_logger.Logs);
    }
}