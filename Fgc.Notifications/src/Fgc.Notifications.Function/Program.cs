using Fgc.Notifications.Function;
using Fgc.Notifications.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// Habilita o modelo isolated com integração ASP.NET Core
builder.ConfigureFunctionsWebApplication();

// Registra a infraestrutura (handlers, options, conexões)
builder.Services.AddInfrastructure();

// Hosted service que declara filas/bindings no RabbitMQ ao subir
builder.Services.AddHostedService<RabbitMqBindingInitializer>();

builder.Build().Run();