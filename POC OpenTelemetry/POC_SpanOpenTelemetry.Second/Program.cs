using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using POC_SpanOpenTelemetry.Core.Helpers;
using POC_SpanOpenTelemetry.Second;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;
        services.RegisterServices(configuration, hostContext.HostingEnvironment.ApplicationName);
        services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .AddSource("*")
                .ConfigureResource(resourceBuilder => resourceBuilder.AddService(hostContext.HostingEnvironment.ApplicationName))
                .AddOtlpExporter(exporterOptions =>
                {
                    exporterOptions.Endpoint = new Uri("http://localhost:4317");
                    exporterOptions.Protocol = OtlpExportProtocol.Grpc;
                }))
            .WithMetrics(builder =>
            {
                builder
                    .AddMeter("*")
                    .ConfigureResource(resourceBuilder => resourceBuilder.AddService(hostContext.HostingEnvironment.ApplicationName))
                    .AddOtlpExporter(exporterOptions =>
                    {
                        exporterOptions.Endpoint = new Uri("http://localhost:4318");
                        exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
            });
        services.AddLogging(builder =>
            builder.SetMinimumLevel(LogLevel.Debug)
                .AddOpenTelemetry(options =>
                {
                    options.AddOtlpExporter( exporterOptions =>
                        {
                            exporterOptions.Endpoint = new Uri("http://localhost:4317");
                            exporterOptions.Protocol = OtlpExportProtocol.Grpc;
                        })
                        .SetResourceBuilder(
                            ResourceBuilder.CreateDefault()
                                .AddService("Logging.NET"))
                        .IncludeScopes = true;
                }));
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();