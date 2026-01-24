namespace GolfScheduler.Api.DTOs;

public record TeeTimeDto(
    Guid Id,
    DateOnly TeeDate,
    TimeOnly TeeTime,
    int MaxPlayers,
    string? Notes,
    DateTime CreatedAt,
    UserDto CreatedBy,
    List<RegistrationDto> Registrations,
    int AvailableSlots
);

public record TeeTimeCreateDto(
    DateOnly TeeDate,
    TimeOnly TeeTime,
    int MaxPlayers = 4,
    string? Notes = null
);

public record TeeTimeUpdateDto(
    DateOnly? TeeDate,
    TimeOnly? TeeTime,
    int? MaxPlayers,
    string? Notes
);

public record TeeTimeListDto(
    Guid Id,
    DateOnly TeeDate,
    TimeOnly TeeTime,
    int MaxPlayers,
    string? Notes,
    int RegisteredCount,
    int AvailableSlots,
    bool IsUserRegistered
);

public record GolfersByDayDto(
    DateOnly Date,
    string DayOfWeek,
    List<TeeTimeWithGolfersDto> TeeTimes
);

public record TeeTimeWithGolfersDto(
    Guid Id,
    TimeOnly TeeTime,
    string? Notes,
    List<string> Golfers
);
