using POC_SpanOpenTelemtry.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC_SpanOpenTelemtry.Mid
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
