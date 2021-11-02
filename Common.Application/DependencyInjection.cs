using System.Reflection;
using VH.MiniService.Common.Application.Behaviors;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace VH.MiniService.Common.Application
{
    /// <summary>
    /// We should only configure things here that we are sure will rarely change and will not be different per service
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds the <see cref="NodaTime"/> system-based <see cref="IClock"/> implementation.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddNodaClock(this IServiceCollection services) =>
            services.AddSingleton<IClock>(SystemClock.Instance);

        /// <summary>
        /// Adds MediatR pipeline and handlers.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration">Containing <see cref="PerformanceOptions"/> and <see cref="UnhandledExceptionOptions"/></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddMediatrPipeline(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
        {
            services.Configure<PerformanceOptions>(configuration.GetSection(PerformanceOptions.SectionName));
            services.Configure<UnhandledExceptionOptions>(configuration.GetSection(UnhandledExceptionOptions.SectionName));

            services.AddMediatR(o => o.AsScoped(), assemblies);
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }

        /// <summary>
        /// Adds MassTransit services.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMassTransitClient(this IServiceCollection services)
        {
            services.AddSingleton<IPublishEndpoint>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<ISendEndpointProvider>(provider => provider.GetRequiredService<IBusControl>());

            return services;
        }
    }
}
