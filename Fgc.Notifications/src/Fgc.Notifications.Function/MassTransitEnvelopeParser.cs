using System.Text.Json;

namespace Fgc.Notifications.Function;

public static class MassTransitEnvelopeParser
{
    public static T Parse<T>(string envelope)
    {
        using var doc = JsonDocument.Parse(envelope);
        var message = doc.RootElement.GetProperty("message");
        return message.Deserialize<T>()!;
    }
}