//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using VH.MiniService.Common.Application.Abstractions;
//using Grpc.Core;
//using Grpc.Core.Interceptors;

//namespace VH.MiniService.Common.Service.Grpc
//{
        // ----------- used HttpRequestContextInterceptor instead ---------------
//    public class RequestContextInterceptor : Interceptor
//    {
//        private readonly IRequestContext _requestContext;

//        public RequestContextInterceptor(IRequestContext requestContext)
//        {
//            _requestContext = requestContext;
//        }

//        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
//        {
//            SetClaims(context);
//            return base.UnaryServerHandler(request, context, continuation);
//        }

//        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
//        {
//            SetClaims(context);
//            return base.ClientStreamingServerHandler(requestStream, context, continuation);
//        }

//        public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
//        {
//            SetClaims(context);
//            return base.ServerStreamingServerHandler(request, responseStream, context, continuation);
//        }

//        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
//        {
//            SetClaims(context);
//            return base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
//        }

//        private void SetClaims(ServerCallContext callContext)
//        {
//            var httpContext = callContext.GetHttpContext();

//            if (_requestContext is not RequestContext requestContext)
//            {
//                throw new Exception($"Current {nameof(IRequestContext)} implementation is not supported ({_requestContext.GetType().FullName})");
//            }

//            if (!httpContext.Request.Headers.TryGetValue(CommonRequestHeaders.TenantId, out var tenantIdStr) ||
//                !int.TryParse(tenantIdStr, out var tenantId))
//            {
//                throw new Exception("Unable to get tenantId from HttpContext!");
//            }

//            requestContext.SetTenantId(tenantId);

//            if (httpContext.User.Identity?.IsAuthenticated ?? false)
//                requestContext.SetClaims(httpContext.User
//                    .Claims
//                    .Select(o => KeyValuePair.Create(o.Type, o.Value)));
//        }
//    }
//}
