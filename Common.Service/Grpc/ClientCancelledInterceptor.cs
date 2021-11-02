using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace VH.MiniService.Common.Service.Grpc
{
    public class ClientCancelledInterceptor : Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await continuation(request, context);
            }
            catch (OperationCanceledException e)
            {
                if (!context.CancellationToken.IsCancellationRequested)
                    throw;

                context.Status = new Status(StatusCode.Cancelled, string.Empty, debugException: e);
                return default!;
            }
        }
    }
}
