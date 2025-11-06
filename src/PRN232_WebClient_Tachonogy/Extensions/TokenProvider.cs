using System.Security.Claims;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;

namespace PRN232_WebClient_Tachonogy.Extensions;

public class TokenProvider(IHttpContextAccessor accessor) : ITokenProvider
{

    private readonly ISession? Session = accessor.HttpContext?.Session;

    public string? AccessToken => Session?.GetString("AccessToken");
    public string? RefreshToken => Session?.GetString("RefreshToken");

    public bool IsAuthenticated => accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public Guid GetUserId()
    {
        var idClaim = accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim?.Value, out var id) ? id : Guid.Empty;
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