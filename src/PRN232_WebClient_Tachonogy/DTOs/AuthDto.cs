using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PRN232_WebClient_Tachonogy.DTOs;

public class LoginRequestDto
{
    [Required] public string Username { get; set; } = string.Empty;
    [PasswordPropertyText] [Required] public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public AuthUserDto User { get; set; } = default!;
}

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthUserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}