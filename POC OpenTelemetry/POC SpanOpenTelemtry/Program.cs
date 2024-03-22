using POC_SpanOpenTelemtry;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

IHost host = Host.CreateDefaultBuilder(args)
.ConfigureServices((hostContext, services) =>
{
    IConfiguration configuration = hostContext.Configuration;
    DependencyInjectionConnector.RegisterServices(services, configuration, hostContext.HostingEnvironment.ApplicationName);
    services.AddOpenTelemetry()
    .WithTracing(builder => builder
    .AddSource("*")
    .ConfigureResource(r => r.AddService(hostContext.HostingEnvironment.ApplicationName))
    .AddOtlpExporter(o =>
    {
        o.Endpoint = new Uri($"http://localhost:4317");
        o.Protocol = OtlpExportProtocol.Grpc;
    }))
    .WithMetrics(builder =>
    {
        builder
        .AddMeter("*")
        .ConfigureResource(r => r.AddService(hostContext.HostingEnvironment.ApplicationName))
        .AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri($"http://localhost:4318");
            o.Protocol = OtlpExportProtocol.HttpProtobuf;
        });
    });
    services.AddLogging(builder =>
    builder.SetMinimumLevel(LogLevel.Debug)
    .AddOpenTelemetry(options =>
    {
        options.AddOtlpExporter(options => { options.Endpoint = new Uri($"http://localhost:4317"); options.Protocol = OtlpExportProtocol.Grpc; })
        .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService("Logging.NET"))
                        .IncludeScopes = true;
    }));
    services.AddHostedService<Worker>();
})
 .Build();

await host.RunAsync();