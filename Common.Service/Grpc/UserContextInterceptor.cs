using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Application.Abstractions;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Common.Service.Grpc
{
    public class UserContextInterceptor : Interceptor
    {
        private readonly IUserContext _userContext;

        public UserContextInterceptor(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            SetClaims(context);
            return base.UnaryServerHandler(request, context, continuation);
        }

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            SetClaims(context);
            return base.ClientStreamingServerHandler(requestStream, context, continuation);
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            SetClaims(context);
            return base.ServerStreamingServerHandler(request, responseStream, context, continuation);
        }

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            SetClaims(context);
            return base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
        }

        private void SetClaims(ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();

            var identity = httpContext.User.Identity;
            if (identity?.IsAuthenticated != true)
                return;

            if (_userContext is ImplicitUserContext userContext)
                userContext.SetClaims(httpContext.User
                    .Claims
                    .Select(o => KeyValuePair.Create(o.Type, o.Value)));
        }
    }
}
