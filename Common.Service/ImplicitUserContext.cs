using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Application.Abstractions;
using IdentityModel;

namespace Common.Service
{
    public class ImplicitUserContext : IUserContext
    {
        private static readonly AsyncLocal<IDictionary<string, string[]>> _claimsStore = new();

        public string GetUserId() => GetUserIdOrDefault() ?? throw new InvalidOperationException("No user id has been set.");

        public string? GetUserIdOrDefault() => GetClaims(JwtClaimTypes.Subject).SingleOrDefault();

        public IEnumerable<string> GetClaims(string claimType)
        {
            var store = _claimsStore.Value;

            if (store is null || !store.TryGetValue(claimType, out var claims))
                return Array.Empty<string>();

            return claims;
        }

        public void SetClaims(IEnumerable<KeyValuePair<string, string>> values)
        {
            var store = _claimsStore.Value;
            store?.Clear();

            _claimsStore.Value = new Dictionary<string, string[]>(values
                .GroupBy(o => o.Key)
                .Select(o => KeyValuePair.Create(o.Key, o.Select(x => x.Value).ToArray())));
        }
    }
}
