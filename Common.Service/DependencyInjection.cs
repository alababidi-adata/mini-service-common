using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using VH.MiniService.Common.Application.Abstractions;
using VH.MiniService.Common.Service.MassTransit;
using VH.MiniService.Common.Service.Options;
using DotNet.Globbing;
using MassTransit;
using MassTransit.Definition;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VH.MiniService.Common.Service
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
            services.AddScoped<IRequestContext, RequestContext>();

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


        public static IServiceCollection AddRedisDistributedCache(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetOptions<RedisOptions>();

            if (!options.Enable) return services;

            return services.AddStackExchangeRedisCache(o =>
            {
                o.InstanceName = options.InstanceName;
                o.Configuration = options.Connection;
            });
        }

        /// <summary>
        /// Adds MassTransit services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">Containing <see cref="MassTransitOptions"/></param>
        /// <param name="assemblies">Assemblies to search in for consumers</param>
        public static IServiceCollection AddMassTransitServices(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
        {
            var options = configuration.GetOptions<MassTransitOptions>();

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

                        cfg.UseConsumeFilter(typeof(ConsumeRequestContextExtractorFilter<>), context);
                        cfg.UsePublishFilter(typeof(PublishRequestContextSetterFilter<>), context);
                        cfg.ConfigureEndpoints(context);
                    });
                });

            return services;
        }

        /// <summary>
        /// Adds controllers as services and OpenApi generation
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureSwaggerGen"></param>
        /// <param name="configureMvc"></param>
        public static IServiceCollection AddWebApi(this IServiceCollection services, Action<SwaggerGenOptions>? configureSwaggerGen = null, Action<IMvcBuilder>? configureMvc = null)
        {
            //services.TryAddScoped<RequestContextMiddleware>();
            DiagnosticListener.AllListeners.Subscribe(new HttpRequestContextInterceptor());

            var mvcBuilder = services
                .AddControllers(o => o.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer())))
                .AddControllersAsServices();
            configureMvc?.Invoke(mvcBuilder);

            services.AddSwaggerGen(o =>
            {
                o.DescribeAllParametersInCamelCase();
                configureSwaggerGen?.Invoke(o);
            });

            return services;
        }

        /// <summary>
        /// Get options from configuration or section by type
        /// With cropping "Options" ending
        /// </summary>
        public static TOptions GetOptions<TOptions>(this IConfiguration section, string? configPath = null)
            where TOptions : class, new()
        {
            const string Ending = "Options";
            var name = typeof(TOptions).Name;
            var sectionName = name.EndsWith(Ending) ? name[..^Ending.Length] : throw new ArgumentException($"{name} must have '{Ending}' ending");
            var fullPath = string.IsNullOrWhiteSpace(configPath) ? sectionName : $"{configPath}:{sectionName}";
            return section.GetSection(fullPath).Get<TOptions>();
        }
    }

    public class KebabCaseParameterTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value) => value == null ? null : Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2").ToLower();
    }
}
