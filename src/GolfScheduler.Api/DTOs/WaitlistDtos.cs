namespace GolfScheduler.Api.DTOs;

public record UserWaitlistEntryDto(
    Guid Id,
    DateOnly TeeDate,
    DateTime JoinedAt,
    int Position
);
