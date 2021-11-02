using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VH.MiniService.Common.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace VH.MiniService.Common.Service.Controllers
{
    public class UserContextMiddleware : IMiddleware
    {
        private readonly IUserContext _userContext;

        public UserContextMiddleware(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (_userContext is ImplicitUserContext userContext)
                userContext.SetClaims(context.User
                    .Claims
                    .Select(o => KeyValuePair.Create(o.Type, o.Value)));

            await next(context);
        }
    }
}
