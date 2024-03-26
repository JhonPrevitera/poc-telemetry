using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POC_SpanOpenTelemtry.Common.Model;

namespace POC_SpanOpenTelemetry.Core.Helpers;

[ExcludeFromCodeCoverage]
public static class DependencyInjectionConnector
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration,
        string clientId)
    {
        services.Configure<ActivitySource>(configuration.GetSection(nameof(ActivitySource)));
        return services;
    }
}