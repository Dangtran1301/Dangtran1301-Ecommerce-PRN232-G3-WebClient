using PRN232_WebClient_Tachonogy.Extensions.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PRN232_WebClient_Tachonogy.Extensions;

public class TokenProvider(IHttpContextAccessor accessor) : ITokenProvider
{
    public string? AccessToken => accessor.HttpContext?.Request.Cookies["AccessToken"];
    public string? RefreshToken => accessor.HttpContext?.Request.Cookies["RefreshToken"];

    public bool IsAuthenticated => accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public Guid GetUserId()
    {
        var accessToken = AccessToken;
        if (string.IsNullOrEmpty(accessToken))
            return Guid.Empty;

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);
        var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(subClaim, out var id) ? id : Guid.Empty;
    }

    public string? GetUserRole() =>
        accessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    public void SetTokens(string accessToken, string refreshToken)
    {
        var response = accessor.HttpContext?.Response;
        if (response == null) return;

        response.Cookies.Append("AccessToken", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(10)
        });

        response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    public void ClearTokens()
    {
        var response = accessor.HttpContext?.Response;
        response?.Cookies.Delete("AccessToken");
        response?.Cookies.Delete("RefreshToken");
    }
}