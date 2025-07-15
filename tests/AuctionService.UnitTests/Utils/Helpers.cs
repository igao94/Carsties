using System.Security.Claims;

namespace AuctionService.UnitTests.Utils;

public class Helpers
{
    public static ClaimsPrincipal GetClaimsPrincipal()
    {
        List<Claim> claims =
        [
            new(ClaimTypes.Name, "test")
        ];

        var identity = new ClaimsIdentity(claims, "testing");

        return new ClaimsPrincipal(identity);
    }
}
