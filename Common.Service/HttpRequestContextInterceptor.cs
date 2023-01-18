using System.Collections.Generic;
using System.Net.Http;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DiagnosticAdapter;

namespace VH.MiniService.Common.Service
{
    public sealed class HttpRequestContextInterceptor : DiagnosticObserverBase
    {
        protected override bool IsMatch(string name) =>
            name is "Microsoft.AspNetCore"
                or "HttpHandlerDiagnosticListener";

        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn.Start")]
        public void OnHttpRequestInStart(HttpContext httpContext)
        {
            var pathValue = httpContext.Request.Path.Value!;
            if (pathValue is "/" or "/healthz" or "/docs/index.html" or "/docs/v1/spec.json") return;

            var headers = httpContext.Request.Headers;
            var token = headers.TryGetValue(CommonRequestHeaders.Token, out var tokenHeader) ? tokenHeader.ToString() : null;
            var tenant = headers.TryGetValue(CommonRequestHeaders.TenantId, out var tenantIdStr)
                ? int.Parse(tenantIdStr!) : (int?)null;

            // TODO: get tenantId from header, validate with permitted tenantIds from token.
            // TODO: consider adding HttpRequestContextInterceptorOptions with restrictions on required fields
            // httpContext.RequestServices.GetService(typeof(x))

            // TODO: Use authorization token from Task 54118: Research and add Authorization to Microservice template (service-template-c-sharp)
            //var claims = httpContext.User.Identity?.IsAuthenticated ?? false
            //    ? httpContext.User.Claims
            //        .GroupBy(o => o.Type)
            //        .ToDictionary(o => o.Key, o => o.Select(x => x.Value).ToArray())
            //    : new Dictionary<string, string[]>();

            var userId = headers.TryGetValue(CommonRequestHeaders.UserId, out var idHeader) ? idHeader.ToString() : null;
            var claims = userId != null
                ? new Dictionary<string, string[]>() { { JwtClaimTypes.Subject, new[] { userId } } }
                : new Dictionary<string, string[]>();

            RequestContext.SetContext(token, claims, tenant);
        }

        [DiagnosticName("System.Net.Http.HttpRequestOut.Start")]
        public void OnHttpRequestOutStart(HttpRequestMessage request)
        {
            var pathValue = request.RequestUri?.PathAndQuery;
            if (pathValue is "/" or "/healthz" or "/docs/index.html" or "/docs/v1/spec.json") return;

            // TODO: Use authorization token from Task 54118: Research and add Authorization to Microservice template (service-template-c-sharp)
            //var token = RequestContext.Value.Token;
            //if (token != null)
            //{
            //    request.Headers.Add(CommonRequestHeaders.Token, token);
            //}

            var c = RequestContext.Value;
            if (c.TenantIdOrDefault != null) request.Headers.Add(CommonRequestHeaders.TenantId, c.TenantIdOrDefault.ToString());
            if (c.UserIdOrDefault != null) request.Headers.Add(CommonRequestHeaders.UserId, c.UserIdOrDefault);
            if (c.Token != null) request.Headers.Add(CommonRequestHeaders.Token, c.Token);
        }

        //[DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn")]
        //public void OnHttpRequestIn()
        //{ }

        //[DiagnosticName("System.Net.Http.HttpRequestOut")]
        //public void OnHttpRequestOut()
        //{ }
    }
}
