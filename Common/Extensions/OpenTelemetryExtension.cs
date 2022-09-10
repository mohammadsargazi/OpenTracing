using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

namespace Common.Extensions
{
    public static class OpenTelemetryExtension
    {
        #region HelperMethods
        private static string? GetAssemblyVersion() =>
            Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        private static string? GetServiceName(this IConfiguration configuration) =>
            configuration.GetValue("UseTracingExporter") switch
            {
                "jaeger" => configuration.GetValue("Jaeger:ServiceName"),
                "zipkin" => configuration.GetValue("Zipkin:ServiceName"),
                "otlp" => configuration.GetValue("Otlp:ServiceName"),
                _ => "AspNetCoreExampleService",
            };
        private static Action<ResourceBuilder> GetResource(this IConfiguration configuration) =>
            r => r.AddService(
                configuration.GetServiceName(), serviceVersion: GetAssemblyVersion(),
                serviceInstanceId: Environment.MachineName);
        private static string GetValue(this IConfiguration configuration, string value) =>
            configuration.GetValue<string>(value).ToLowerInvariant();
        #endregion

        public static void AddOpenTelemetryTracing(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOpenTelemetryTracing(options =>
            {
                options
                      .ConfigureResource(configuration.GetResource())
                      .SetSampler(new AlwaysOnSampler())
                      .AddHttpClientInstrumentation()
                      .AddAspNetCoreInstrumentation();

                switch (configuration.GetValue("UseTracingExporter"))
                {
                    case "jaeger":
                        options.AddJaegerExporter();

                        services.Configure<JaegerExporterOptions>(configuration.GetSection("Jaeger"));

                        // Customize the HttpClient that will be used when JaegerExporter is configured for HTTP transport.
                        services.AddHttpClient("JaegerExporter", configureClient: (client) => client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value"));
                        break;

                    case "zipkin":
                        options.AddZipkinExporter();

                        services.Configure<ZipkinExporterOptions>(configuration.GetSection("Zipkin"));
                        break;

                    case "otlp":
                        options.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(configuration.GetValue("Otlp:Endpoint"));
                        });
                        break;

                    default:
                        options.AddConsoleExporter();

                        break;
                }
            });

            // For options which can be bound from IConfiguration.
            services.Configure<AspNetCoreInstrumentationOptions>(configuration.GetSection("AspNetCoreInstrumentation"));

        }

        public static void AddOpenTelemetryLoggin(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            builder.Logging.AddOpenTelemetry(options =>
            {
                options.ConfigureResource(configuration.GetResource());

                switch (configuration.GetValue("UseLogExporter"))
                {
                    case "otlp":
                        options.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(builder.Configuration.GetValue("Otlp:Endpoint"));
                        });
                        break;
                    default:
                        options.AddConsoleExporter();
                        break;
                }
            });

            builder.Services.Configure<OpenTelemetryLoggerOptions>(opt =>
            {
                opt.IncludeScopes = true;
                opt.ParseStateValues = true;
                opt.IncludeFormattedMessage = true;
            });
        }

        public static void AddOpenTelemetryMetrics(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddOpenTelemetryMetrics(options =>
            {
                options.ConfigureResource(configuration.GetResource())
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();

                switch (configuration.GetValue("UseMetricsExporter"))
                {
                    case "prometheus":
                        options.AddPrometheusExporter();
                        break;
                    case "otlp":
                        options.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(configuration.GetValue("Otlp:Endpoint"));
                        });
                        break;
                    default:
                        options.AddConsoleExporter();
                        break;
                }
            });
        }

      
    }
}
