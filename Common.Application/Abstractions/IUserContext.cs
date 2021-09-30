using System.Collections.Generic;
using System.Linq;
using IdentityModel;

namespace Common.Application.Abstractions
{
    public interface IUserContext
    {
        public string GetUserId();
        public string? GetUserIdOrDefault();
        
        /// <summary>
        /// Returns all claim values of the specified <paramref name="claimType"/>.
        /// </summary>
        /// <param name="claimType"></param>
        /// <returns></returns>
        public IEnumerable<string> GetClaims(string claimType);
        
        /// <summary>
        /// Returns the first found claim with the specified <paramref name="claimType"/> - or null if none found.
        /// </summary>
        /// <param name="claimType"></param>
        /// <returns></returns>
        public string? GetFirstClaim(string claimType) => GetClaims(claimType).FirstOrDefault();

        /// <summary>
        /// Returns the first found 'email' claim - or null if none found.
        /// </summary>
        /// <returns></returns>
        public string? GetEmailClaim() => GetFirstClaim(JwtClaimTypes.Email);
    }
}
