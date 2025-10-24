using PRN232_WebClient_Tachonogy.Extensions.Interfaces;

namespace PRN232_WebClient_Tachonogy.Extensions;

public class TokenProvider(IHttpContextAccessor accessor) : ITokenProvider
{
    private HttpResponse? Response => accessor.HttpContext?.Response;
    private HttpRequest? Request => accessor.HttpContext?.Request;

    public string? AccessToken => Request?.Cookies["AccessToken"];
    public string? RefreshToken => Request?.Cookies["RefreshToken"];

    public void SetTokens(string accessToken, string refreshToken)
    {
        Response?.Cookies.Append("AccessToken", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });

        Response?.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    public void ClearTokens()
    {
        Response?.Cookies.Delete("AccessToken");
        Response?.Cookies.Delete("RefreshToken");
    }
}