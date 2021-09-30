using System;
using System.Reflection;
using Common.Application.Abstractions;
using Common.Service.MassTransit;
using DotNet.Globbing;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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
                .AddAspNetCoreInstrumentation(c => c.RecordException = true)
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
                .AddJaegerExporter()
                );

        /// <summary>
        /// Adds MassTransit services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">Configuration holding a 'RabbitMQ:Host/Port/Username/Password' value.</param>
        /// <param name="assemblies">Assemblies to search in for consumers.</param>
        /// <returns></returns>
        public static IServiceCollection AddMassTransitServices(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
        {
            // Register MassTransit consumers
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(o => o.AssignableTo<IConsumer>())
                .AsSelf()
                .WithScopedLifetime());

            services
                .AddMassTransitHostedService()
                .AddMassTransit(o =>
                {
                    o.AddConsumers(assemblies);

                    o.UsingRabbitMq((context, cfg) =>
                    {
                        var host = $"{configuration["RabbitMQ:Host"]}:{configuration["RabbitMQ:Port"]}";
                        cfg.Host(new Uri($"rabbitmq://{host}"), rabbit =>
                        {
                            rabbit.PublisherConfirmation = true;
                            rabbit.Username(configuration["RabbitMQ:Username"]);
                            rabbit.Password(configuration["RabbitMQ:Password"]);
                        });

                        cfg.UseConsumeFilter(typeof(SentryConsumeFilter<>), context);
                        cfg.ConfigureEndpoints(context);
                    });
                });

            return services;
        }
    }
}
