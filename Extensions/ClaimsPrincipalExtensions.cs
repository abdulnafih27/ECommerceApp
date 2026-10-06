using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static int GetCurrentUser(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User id claim not found in token.");
        return int.Parse(claim.Value);
    }
}