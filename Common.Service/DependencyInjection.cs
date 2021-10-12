using System;
using System.Reflection;
using Common.Application.Abstractions;
using Common.Service.MassTransit;
using Common.Service.Options;
using DotNet.Globbing;
using MassTransit;
using MassTransit.Definition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RabbitMQ.Client;

namespace Common.Service
{
    /// <summary>
    /// We should only configure things here that we are sure will rarely change and will not be different per service
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds the core transport services for a given app service.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddServiceCore(this IServiceCollection services)
        {
            services.AddScoped<IUserContext, ImplicitUserContext>();

            return services;
        }

        /// <summary>
        /// Registers services matching the <paramref name="searchPattern"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="searchPattern">The glob to search for. Default: '*Service'</param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterClasses(this IServiceCollection services, string searchPattern = "*Service", params Assembly[] assemblies)
        {
            var glob = Glob.Parse(searchPattern);

            //Register *Service.cs classes
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(x => x.Where(type => glob.IsMatch(type.Name)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }

        public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration, string serviceName) =>
            // TODO: Consider adding TracingResourceFilter if needed from https://medium.com/@bacheric/observability-with-opentelemetry-205adb984792
            services.AddOpenTelemetryTracing(o => o
                .SetSampler(new AlwaysOnSampler())
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                .SetErrorStatusOnException()
                .AddAspNetCoreInstrumentation(c =>
                {
                    c.RecordException = true;
                    c.EnableGrpcAspNetCoreSupport = true;
                })
                .AddHttpClientInstrumentation(c =>
                {
                    c.SetHttpFlavor = true;
                    c.RecordException = true;
                })
                .AddGrpcClientInstrumentation()
                .AddMassTransitInstrumentation()
                .AddSqlClientInstrumentation(opt =>
                {
                    // Enabled all, will disable later when evaluate what is not needed
                    opt.EnableConnectionLevelAttributes = true;
                    opt.RecordException = true;
                    opt.SetDbStatementForStoredProcedure = true;
                    opt.SetDbStatementForText = true;
                })
                .AddJaegerExporter(opts =>
                {
                    // TODO: add Jaeger exporter config section
                    //opts.AgentHost = Configuration["Jaeger:AgentHost"];
                    //opts.AgentPort = Convert.ToInt32(Configuration["Jaeger:AgentPort"]);
                })
                );

        /// <summary>
        /// Adds MassTransit services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options">configuration options</param>
        /// <param name="assemblies">Assemblies to search in for consumers</param>
        /// <returns></returns>
        public static IServiceCollection AddMassTransitServices(this IServiceCollection services, MassTransitOptions options, params Assembly[] assemblies)
        {
            if (options == null) throw new Exception($"'{nameof(MassTransitOptions)}' must not be null!");

            if (!options.Enable) return services;

            // Register MassTransit consumers
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(o => o.AssignableTo<IConsumer>())
                .AsSelf()
                .WithScopedLifetime());

            var rmqOptions = options.RabbitMq ?? throw new Exception($"{nameof(options.RabbitMq)}' must not be null!");

            var host = $"{rmqOptions.Host}:{rmqOptions.Port}";

            services
                .AddMassTransitHostedService()
                .AddMassTransit(o =>
                {
                    o.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(true));
                    o.AddConsumers(assemblies);
                    o.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(new Uri($"rabbitmq://{host}"), rabbit =>
                        {
                            rabbit.PublisherConfirmation = true;
                            rabbit.Username(rmqOptions.Username);
                            rabbit.Password(rmqOptions.Password);
                        });

                        cfg.UseConsumeFilter(typeof(ExtendedConsumeFilter<>), context);
                        cfg.ConfigureEndpoints(context);
                    });
                });

            if (!rmqOptions.HealthChecks.Enable) return services;

            services.AddHealthChecks()
                .AddRabbitMQ($"amqp://{rmqOptions.Username}:{rmqOptions.Password}@{host}",
                new SslOption(),
                tags: rmqOptions.HealthChecks.Tags);

            return services;
        }

        /// <summary>
        /// Get options from configuration or section by type
        /// With cropping "Options" ending
        /// </summary>
        public static TOptions GetOptions<TOptions>(this IConfiguration section, string? configPath = null)
            where TOptions : class, new()
        {
            var name = typeof(TOptions).Name;
            const string Ending = "Options";
            var sectionName = name.EndsWith(Ending) ? name[..^Ending.Length] : name;
            var path = string.IsNullOrWhiteSpace(configPath) ? "" : $"{configPath}:";
            return section.GetSection($"{path}{sectionName}").Get<TOptions>();
        }
    }
}
