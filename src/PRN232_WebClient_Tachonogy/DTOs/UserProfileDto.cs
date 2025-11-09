using System.ComponentModel.DataAnnotations;

namespace PRN232_WebClient_Tachonogy.DTOs;

public record UserProfileDto(
    Guid Id,
    string FullName,
    string? PhoneNumber,
    string? Avatar,
    Gender Gender,
    DateTime? DayOfBirth,
    string? Address
);

public record UpdateUserProfileRequest(
    [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters")]
    string? FullName,

    [Phone(ErrorMessage = "Invalid phone number format")]
    string? PhoneNumber,

    [StringLength(255, ErrorMessage = "Avatar URL cannot exceed 255 characters")]
    string? Avatar,

    Gender? Gender = null,

    DateTime? DayOfBirth = null,

    string? Address = null
);
