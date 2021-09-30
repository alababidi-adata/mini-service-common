using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Common.Service.Controllers
{
    public class RequestAbortedMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (OperationCanceledException)
            {
                if (!context.RequestAborted.IsCancellationRequested)
                    throw;
            }
        }
    }
}
