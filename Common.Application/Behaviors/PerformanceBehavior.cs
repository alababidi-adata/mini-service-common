using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VH.MiniService.Common.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace VH.MiniService.Common.Application.Behaviors
{
    public class PerformanceOptions
    {
        public const string SectionName = "Performance";
        public TimeSpan WarningThreshold { get; set; } = TimeSpan.FromMinutes(1);
    }

    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IClock _clock;
        private readonly IUserContext _userContext;
        private readonly IOptions<PerformanceOptions> _options;
        private readonly ILogger<TRequest> _logger;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ActivitySource _activitySource = new("Application.Performance");


        public PerformanceBehavior(IClock clock, IUserContext userContext, IOptions<PerformanceOptions> options, ILogger<TRequest> logger)
        {
            _clock = clock;
            _userContext = userContext;
            _options = options;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
        {
            var requestName = typeof(TRequest).Name;
            using var activity = _activitySource.StartActivity(requestName);

            var response = await next();

            activity?.SetEndTime(_clock.GetCurrentInstant().ToDateTimeUtc());

            if (activity is null || activity.Duration <= _options.Value.WarningThreshold)
                return response;

            var userId = _userContext.GetUserIdOrDefault() ?? string.Empty;

            _logger.LogWarning("Long Running Request [MiniService]: {Name} ({ElapsedMilliseconds} milliseconds) {UserId} {Request}",
                requestName, activity.Duration.Milliseconds, userId, request);

            return response;
        }
    }
}
