using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace PRN232_WebClient_Tachonogy.Middlewares;

public class TokenRefreshMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITokenProvider tokenProvider, IHttpClientFactory httpClientFactory)
    {
        var accessToken = tokenProvider.AccessToken;
        var refreshToken = tokenProvider.RefreshToken;

        if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.ReadJwtToken(accessToken);

            if (jwt.ValidTo < DateTime.UtcNow.AddMinutes(2))
            {
                try
                {
                    var client = httpClientFactory.CreateClient("AuthApi");
                    var dto = new RefreshTokenRequestDto { RefreshToken = refreshToken };
                    var response = await client.PostAsJsonAsync("auth/refresh", dto);

                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

                    if (result?.Success == true && result.Data is not null)
                    {
                        tokenProvider.SetTokens(result.Data.AccessToken, result.Data.RefreshToken);
                    }
                    else
                    {
                        tokenProvider.ClearTokens();
                    }
                }
                catch
                {
                    tokenProvider.ClearTokens();
                }
            }
        }

        await next(context);
    }
}