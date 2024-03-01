﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using POC_SpanOpenTelemtry.Common;

namespace POC_SpanOpenTelemtry
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjectionConnector
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration, string clientId)
        {
            services.Configure<ActivitySource>(configuration.GetSection(nameof(ActivitySource)));
            return services;
        }
    }
}
