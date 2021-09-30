using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Common.Application.Behaviors
{
    public class PerformanceBehavior<TRequest, TResponse> : PerformanceBehavior, IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IClock _clock;
        private readonly IUserContext _userContext;
        private readonly IOptions<PerformanceOptions> _options;
        private readonly ILogger<TRequest> _logger;

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
            using var activity = ActivitySource.StartActivity(requestName);

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

    // Needed so every PerformanceBehavior<,> shares the same activity source.
    public class PerformanceBehavior
    {
        protected static ActivitySource ActivitySource { get; } = new("Application.Performance");
    }
}
