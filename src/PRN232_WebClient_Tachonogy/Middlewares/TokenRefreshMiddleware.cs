using PRN232_WebClient_Tachonogy.DTOs;
using PRN232_WebClient_Tachonogy.Extensions.Interfaces;
using PRN232_WebClient_Tachonogy.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace PRN232_WebClient_Tachonogy.Middlewares;

public class TokenRefreshMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITokenProvider tokenProvider, IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        if (context.Request.Path.StartsWithSegments("/auth/refresh", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var accessToken = tokenProvider.AccessToken;
        var refreshToken = tokenProvider.RefreshToken;

        if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
        {

            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwt = jwtHandler.ReadJwtToken(accessToken);

                if (jwt.ValidTo < DateTime.UtcNow.AddMinutes(2))
                {
                    var dto = new RefreshTokenRequestDto { RefreshToken = refreshToken };
                    var result = await authService.RefreshAsync(dto);

                    if (result is { Success: true, Data: not null })
                    {
                        tokenProvider.SetTokens(result.Data.AccessToken, result.Data.RefreshToken);
                    }
                    else
                    {
                        tokenProvider.ClearTokens();
                    }
                }
            }
            catch
            {
                tokenProvider.ClearTokens();
            }
        }

        await next(context);
    }
}