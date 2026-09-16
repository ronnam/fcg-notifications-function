using System;
using Fgc.MessageContracts.Events;
using Fgc.Notifications.Application.Handlers;
using Fgc.Notifications.UnitTests.Helpers;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Fgc.Notifications.UnitTests.Handlers;

public class UserCreatedHandlerTests
{
    private readonly TestLogger<UserCreatedHandler> _logger = new();
    private readonly UserCreatedHandler _handler;

    public UserCreatedHandlerTests()
    {
        _handler = new UserCreatedHandler(_logger);
    }

    [Fact]
    public void Handle_LogaEmailDeBoasVindasComDadosDoUsuario()
    {
        var @event = new UserCreatedEvent(
            Id: Guid.NewGuid(),
            Name: "Ronnam",
            Email: "teste@fcg.com",
            CreatedAt: DateTime.UtcNow);

        _handler.Handle(@event).GetAwaiter().GetResult();

        var log = Assert.Single(_logger.Logs);
        Assert.Equal(LogLevel.Information, log.Level);
        Assert.Contains("WELCOME EMAIL", log.Message);
        Assert.Contains("teste@fcg.com", log.Message);
        Assert.Contains("Ronnam", log.Message);
    }
}