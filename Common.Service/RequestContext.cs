using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VH.MiniService.Common.Application.Abstractions;
using IdentityModel;

namespace VH.MiniService.Common.Service
{

    public class RequestContext : IRequestContext
    {
        protected internal record Context(string? Token, int? TenantIdOrDefault, IDictionary<string, string[]> Claims, string? UserIdOrDefault);
        protected internal static Context Value => _context.Value!;
        protected static readonly AsyncLocal<Context> _context = new();

        public string? UserIdOrDefault => _context.Value!.UserIdOrDefault;
        public string UserIdOrThrow => UserIdOrDefault ?? throw new InvalidOperationException($"{nameof(UserIdOrThrow)}: No user id has been set.");
        public int? TenantIdOrDefault => _context.Value!.TenantIdOrDefault;
        public int TenantIdOrThrow => TenantIdOrDefault ?? throw new InvalidOperationException($"{nameof(TenantIdOrThrow)}: No tenant id has been set.");
        public string? Token => _context.Value!.Token;
        public IDictionary<string, string[]> AllClaims => _context.Value!.Claims;

        public string[] GetClaimData(string claimType) =>
            _context.Value!.Claims.TryGetValue(claimType, out var claims)
                ? claims
                : Array.Empty<string>();

        internal static void SetContext(string? token, Dictionary<string, string[]> claims, int? tenantId)
        {
            var userId = claims.TryGetValue(JwtClaimTypes.Subject, out var sub) ? sub.SingleOrDefault() : null;

            _context.Value = new Context(token, tenantId, claims, userId);
        }
    }
}
