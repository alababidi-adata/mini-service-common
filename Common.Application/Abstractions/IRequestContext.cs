using System.Linq;
using IdentityModel;

namespace VH.MiniService.Common.Application.Abstractions
{
    public interface IRequireUser { }
    public interface IRequireTenant { }

    public interface IRequestContext
    {
        int TenantIdOrThrow { get; }
        int? TenantIdOrDefault { get; }

        public string UserIdOrThrow { get; }
        public string? UserIdOrDefault { get; }

        public string? EmailOrDefault => GetFirstClaim(JwtClaimTypes.Email);

        public string[] GetClaimData(string claimType);
        public string? GetFirstClaim(string claimType) => GetClaimData(claimType).FirstOrDefault();
    }
}
