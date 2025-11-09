using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PRN232_WebClient_Tachonogy.DTOs;

public class LoginRequestDto
{
    [Required] public string Username { get; set; } = string.Empty;
    [PasswordPropertyText][Required] public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    [Required]
    public string AccessToken { get; set; } = default!;

    [Required]
    public string RefreshToken { get; set; } = default!;

    public DateTime ExpiresAt { get; set; }

    [Required]
    public AuthUserResponse User { get; set; } = default!;
}

public record RegisterRequest(
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long")]
    string UserName,

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    string Email,

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    string Password,

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    string FullName
);

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthUserResponse
{
    public Guid UserId { get; set; }

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [Required]
    public string AccountStatus { get; set; } = string.Empty;
}

public record RemoteAuthUserRequest(
    [Required(ErrorMessage = "Role is required")]
    string Role
);

public record UpdateStatusAuthUserRequest(
    [Required(ErrorMessage = "New status is required")]
    string NewStatus
);

public record ForgotPasswordRequest(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    string Email,
    string? ClientUri = null
);

public record ResetPasswordRequest(
    [Required(ErrorMessage = "Token is required")]
    string Token,

    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "New password must be at least 6 characters long")]
    string NewPassword
);

public class ChangePasswordRequest
{
    [Required]
    public string OldPassword { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your new password")]
    [Compare(otherProperty: nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
};

public class UserFilterRequest
{
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string? UserName { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    public Role? Role { get; set; }

    public AccountStatus? AccountStatus { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Skip must be non-negative")]
    public int? Skip { get; set; }

    [Range(1, 1000, ErrorMessage = "Take must be between 1 and 1000")]
    public int? Take { get; set; }

    [StringLength(50, ErrorMessage = "OrderBy cannot exceed 50 characters")]
    public string? OrderBy { get; set; }

    public bool Descending { get; set; } = false;
}


public enum Role
{
    Admin,
    Seller,
    Customer
}

public enum Gender
{
    Male,
    Female,
    Unknown
}

public enum AccountStatus
{
    Active,
    Inactive,
}