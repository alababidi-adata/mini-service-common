//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using VH.MiniService.Common.Application.Abstractions;
//using Microsoft.AspNetCore.Http;

//namespace VH.MiniService.Common.Service.Controllers
//{
//    public class RequestContextMiddleware : IMiddleware
//    {
//        private readonly IRequestContext _requestContext;

//        public RequestContextMiddleware(IRequestContext requestContext)
//        {
//            _requestContext = requestContext;
//        }

//        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
//        {
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

//            await next(httpContext);
//        }
//    }
//}
