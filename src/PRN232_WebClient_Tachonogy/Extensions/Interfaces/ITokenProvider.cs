namespace PRN232_WebClient_Tachonogy.Extensions.Interfaces;

public interface ITokenProvider
{
    string? AccessToken { get; }
    string? RefreshToken { get; }

    bool IsAuthenticated { get; }

    Guid GetUserId();
    string? GetUserRole();

    void SetTokens(string accessToken, string refreshToken);
    void ClearTokens();
}