namespace PRN232_WebClient_Tachonogy.Extensions.Interfaces;

public interface ITokenProvider
{
    string? AccessToken { get; }
    string? RefreshToken { get; }
    void SetTokens(string accessToken, string refreshToken);
    void ClearTokens();
}