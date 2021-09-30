using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Common.Application.Behaviors
{
    public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;

                ex.Data.Add(nameof(request), request);
                ex.Data.Add(nameof(requestName), requestName);

                throw;
            }
        }
    }
}
