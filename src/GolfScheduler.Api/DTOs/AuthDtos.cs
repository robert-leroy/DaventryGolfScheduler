using System.ComponentModel.DataAnnotations;

namespace GolfScheduler.Api.DTOs;

public record LoginRequestDto(
    [Required][EmailAddress] string Email,
    [Required][MinLength(8)] string Password
);

public record RegisterRequestDto(
    [Required][EmailAddress][MaxLength(255)] string Email,
    [Required][MinLength(8)] string Password,
    [Required][MaxLength(100)] string FirstName,
    [Required][MaxLength(100)] string LastName,
    [MaxLength(20)] string? Phone = null,
    int? Handicap = null
);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserProfileDto User
);

public record RefreshTokenRequestDto(
    [Required] string RefreshToken
);

public record ChangePasswordRequestDto(
    [Required] string CurrentPassword,
    [Required][MinLength(8)] string NewPassword
);
