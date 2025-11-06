using System.Security.Claims;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;

namespace PRN232_WebClient_Tachonogy.Extensions;

public class TokenProvider(IHttpContextAccessor accessor) : ITokenProvider
{
    private HttpResponse? Response => accessor.HttpContext?.Response;
    private HttpRequest? Request => accessor.HttpContext?.Request;

    public string? AccessToken => accessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
    public string? RefreshToken => accessor.HttpContext?.User?.FindFirst("RefreshToken")?.Value;

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
            Expires = DateTimeOffset.UtcNow.AddHours(1)
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