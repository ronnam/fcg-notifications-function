using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Fgc.Notifications.Function;

public class HealthFunction(ILogger<HealthFunction> logger)
{
    [Function("Health")]
    public HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        logger.LogInformation("Health check executado.");
        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        response.WriteString("OK");
        return response;
    }
}