using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fgc.Notifications.Function;

public static class MassTransitEnvelopeParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString

    };

    public static T Parse<T>(string envelope)
    {
        using var doc = JsonDocument.Parse(envelope);
        var message = doc.RootElement.GetProperty("message");
        return message.Deserialize<T>(Options)!;
    }
}