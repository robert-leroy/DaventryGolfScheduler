namespace GolfScheduler.Api.DTOs;

public record RegistrationDto(
    Guid Id,
    Guid UserId,
    string UserDisplayName,
    DateTime RegisteredAt
);

public record UserRegistrationDto(
    Guid Id,
    Guid TeeTimeId,
    DateOnly TeeDate,
    TimeOnly TeeTime,
    DateTime RegisteredAt
);
