using Fgc.Notifications.Function;
using Fgc.Notifications.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddInfrastructure();
        services.AddHostedService<RabbitMqBindingInitializer>();
    })
    .Build();

host.Run();