namespace GolfScheduler.Api.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string DisplayName,
    string? Phone,
    int? Handicap,
    bool IsAdmin
);

public record UserProfileDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string DisplayName,
    string? Phone,
    int? Handicap,
    bool IsAdmin,
    DateTime CreatedAt
);

public record UserAdminUpdateDto(
    bool IsAdmin
);

public record UserProfileUpdateDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    int? Handicap
);

public record UserCreateDto(
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    int? Handicap,
    bool IsAdmin = false
);

public record UserUpdateDto(
    string? Email,
    string? FirstName,
    string? LastName,
    string? Phone,
    int? Handicap,
    bool? IsAdmin
);
