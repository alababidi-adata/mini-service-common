using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VH.MiniService.Common.Errors;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VH.MiniService.Common.Application.Behaviors
{
    public class UnhandledExceptionOptions
    {
        public const string SectionName = "UnhandledException";
        public bool AppendExceptionDetailsToError { get; set; } = false;
    }

    public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : ResultBase, new()
    {
        private readonly bool _outputExceptionDetailsToError;
        private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

        public UnhandledExceptionBehavior(IOptions<UnhandledExceptionOptions> options, ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
            _outputExceptionDetailsToError = options.Value.AppendExceptionDetailsToError;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var errorLogId = Guid.NewGuid();
                var exType = ex is AggregateException aggEx
                    ? string.Join("+", aggEx.InnerExceptions.Select(e => e.GetType().Name))
                    : ex.GetType().Name;
                var requestName = typeof(TRequest).Name;
                ex.Data.Add(nameof(request), request);
                ex.Data.Add(nameof(requestName), requestName);

                _logger.LogError(ex, "Unhandled error occurred during handling '{RequestName}', ErrorLogId: '{ErrorLogId}', Type: {ErrorType}", requestName, errorLogId, exType);

                var result = new TResponse();
                result.Reasons.Add(new UnknownError(
                    localizedMessage: $"Unhandled error occurred, ErrorId: '{errorLogId}'",
                    internalMessage: $"ErrorId: '{errorLogId}' during handling {requestName}."
                                     + (_outputExceptionDetailsToError
                                         ? $"{Environment.NewLine}---{Environment.NewLine} {ex} {Environment.NewLine}---{Environment.NewLine}"
                                         : "")
                    ));

                return result;
            }
        }
    }
}
