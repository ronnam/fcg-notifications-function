using Fgc.Notifications.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Fgc.Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<UserCreatedHandler>();
        services.AddTransient<PaymentProcessedHandler>();
        return services;
    }
}