using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GolfScheduler.Api.Data;
using GolfScheduler.Api.DTOs;
using GolfScheduler.Api.Models;
using GolfScheduler.Api.Services;
using System.Security.Claims;

namespace GolfScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeeTimesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;

    public TeeTimesController(AppDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TeeTimeListDto>>> GetTeeTimes()
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var teeTimes = await _context.TeeTimes
            .Include(t => t.Registrations)
            .Where(t => t.TeeDate >= today)
            .OrderBy(t => t.TeeDate)
            .ThenBy(t => t.TeeTimeValue)
            .ToListAsync();

        var activeDates = teeTimes.Select(t => t.TeeDate).Distinct().ToList();
        var waitlistEntries = await _context.WaitlistEntries
            .Where(w => activeDates.Contains(w.TeeDate))
            .OrderBy(w => w.JoinedAt)
            .ToListAsync();

        var teeTimesByDate = teeTimes.GroupBy(t => t.TeeDate)
            .ToDictionary(g => g.Key, g => g.ToList());
        var waitlistByDate = waitlistEntries.GroupBy(w => w.TeeDate)
            .ToDictionary(g => g.Key, g => g.ToList());

        var userRegisteredDates = teeTimes
            .Where(t => t.Registrations.Any(r => r.UserId == currentUser.Id))
            .Select(t => t.TeeDate)
            .ToHashSet();

        var result = teeTimes.Select(t =>
        {
            var dayTeeTimes = teeTimesByDate[t.TeeDate];
            var isDayFull = dayTeeTimes.All(dt => dt.Registrations.Count >= dt.MaxPlayers);

            var dayWaitlist = waitlistByDate.TryGetValue(t.TeeDate, out var wl)
                ? wl.OrderBy(w => w.JoinedAt).ToList()
                : new List<WaitlistEntry>();
            var waitlistIndex = dayWaitlist.FindIndex(w => w.UserId == currentUser.Id);

            return new TeeTimeListDto(
                t.Id,
                t.TeeDate,
                t.TeeTimeValue,
                t.MaxPlayers,
                t.Notes,
                t.Registrations.Count,
                t.MaxPlayers - t.Registrations.Count,
                t.Registrations.Any(r => r.UserId == currentUser.Id),
                userRegisteredDates.Contains(t.TeeDate),
                isDayFull,
                waitlistIndex >= 0,
                waitlistIndex >= 0 ? waitlistIndex + 1 : null,
                dayWaitlist.Count
            );
        }).ToList();

        return Ok(result);
    }

    [HttpGet("by-day")]
    public async Task<ActionResult<List<GolfersByDayDto>>> GetGolfersByDay()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var teeTimes = await _context.TeeTimes
            .Include(t => t.Registrations)
                .ThenInclude(r => r.User)
            .Where(t => t.TeeDate >= today)
            .OrderBy(t => t.TeeDate)
            .ThenBy(t => t.TeeTimeValue)
            .ToListAsync();

        var grouped = teeTimes
            .GroupBy(t => t.TeeDate)
            .Select(g => new GolfersByDayDto(
                g.Key,
                g.Key.DayOfWeek.ToString(),
                g.Select(t => new TeeTimeWithGolfersDto(
                    t.Id,
                    t.TeeTimeValue,
                    t.Notes,
                    t.Registrations.Select(r => r.User.DisplayName).ToList()
                )).ToList()
            ))
            .ToList();

        return Ok(grouped);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TeeTimeDto>> GetTeeTime(Guid id)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var teeTime = await _context.TeeTimes
            .Include(t => t.CreatedBy)
            .Include(t => t.Registrations)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teeTime == null) return NotFound();

        var dayTeeTimes = await _context.TeeTimes
            .Include(t => t.Registrations)
            .Where(t => t.TeeDate == teeTime.TeeDate)
            .ToListAsync();

        var isDayFull = dayTeeTimes.All(dt => dt.Registrations.Count >= dt.MaxPlayers);
        var isUserRegisteredForDay = dayTeeTimes.Any(dt => dt.Registrations.Any(r => r.UserId == currentUser.Id));

        var dayWaitlist = await _context.WaitlistEntries
            .Where(w => w.TeeDate == teeTime.TeeDate)
            .OrderBy(w => w.JoinedAt)
            .ToListAsync();

        var waitlistIndex = dayWaitlist.FindIndex(w => w.UserId == currentUser.Id);

        var dto = new TeeTimeDto(
            teeTime.Id,
            teeTime.TeeDate,
            teeTime.TeeTimeValue,
            teeTime.MaxPlayers,
            teeTime.Notes,
            teeTime.CreatedAt,
            MapToUserDto(teeTime.CreatedBy),
            teeTime.Registrations.Select(r => new RegistrationDto(
                r.Id,
                r.UserId,
                r.User.DisplayName,
                r.RegisteredAt
            )).ToList(),
            teeTime.MaxPlayers - teeTime.Registrations.Count,
            isUserRegisteredForDay,
            isDayFull,
            waitlistIndex >= 0,
            waitlistIndex >= 0 ? waitlistIndex + 1 : null,
            dayWaitlist.Count
        );

        return Ok(dto);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<TeeTimeDto>> CreateTeeTime([FromBody] TeeTimeCreateDto dto)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var teeTime = new TeeTime
        {
            Id = Guid.NewGuid(),
            TeeDate = dto.TeeDate,
            TeeTimeValue = dto.TeeTime,
            MaxPlayers = dto.MaxPlayers,
            Notes = dto.Notes,
            CreatedById = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TeeTimes.Add(teeTime);
        await _context.SaveChangesAsync();

        var result = new TeeTimeDto(
            teeTime.Id,
            teeTime.TeeDate,
            teeTime.TeeTimeValue,
            teeTime.MaxPlayers,
            teeTime.Notes,
            teeTime.CreatedAt,
            MapToUserDto(currentUser),
            new List<RegistrationDto>(),
            teeTime.MaxPlayers,
            false,
            false,
            false,
            null,
            0
        );

        return CreatedAtAction(nameof(GetTeeTime), new { id = teeTime.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<TeeTimeDto>> UpdateTeeTime(Guid id, [FromBody] TeeTimeUpdateDto dto)
    {
        var teeTime = await _context.TeeTimes
            .Include(t => t.CreatedBy)
            .Include(t => t.Registrations)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teeTime == null) return NotFound();

        if (dto.TeeDate.HasValue) teeTime.TeeDate = dto.TeeDate.Value;
        if (dto.TeeTime.HasValue) teeTime.TeeTimeValue = dto.TeeTime.Value;
        if (dto.MaxPlayers.HasValue) teeTime.MaxPlayers = dto.MaxPlayers.Value;
        if (dto.Notes != null) teeTime.Notes = dto.Notes;
        teeTime.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var result = new TeeTimeDto(
            teeTime.Id,
            teeTime.TeeDate,
            teeTime.TeeTimeValue,
            teeTime.MaxPlayers,
            teeTime.Notes,
            teeTime.CreatedAt,
            MapToUserDto(teeTime.CreatedBy),
            teeTime.Registrations.Select(r => new RegistrationDto(
                r.Id,
                r.UserId,
                r.User.DisplayName,
                r.RegisteredAt
            )).ToList(),
            teeTime.MaxPlayers - teeTime.Registrations.Count,
            false,
            false,
            false,
            null,
            0
        );

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteTeeTime(Guid id)
    {
        var teeTime = await _context.TeeTimes.FindAsync(id);
        if (teeTime == null) return NotFound();

        _context.TeeTimes.Remove(teeTime);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/register")]
    public async Task<IActionResult> Register(Guid id)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var teeTime = await _context.TeeTimes
            .Include(t => t.Registrations)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teeTime == null) return NotFound();

        if (teeTime.Registrations.Any(r => r.UserId == currentUser.Id))
            return BadRequest("Already registered for this tee time");

        var alreadyRegisteredToday = await _context.Registrations
            .Include(r => r.TeeTime)
            .AnyAsync(r => r.UserId == currentUser.Id && r.TeeTime.TeeDate == teeTime.TeeDate);

        if (alreadyRegisteredToday)
            return BadRequest("You already have a tee time booked for this day");

        if (teeTime.Registrations.Count >= teeTime.MaxPlayers)
            return BadRequest("Tee time is full");

        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            TeeTimeId = id,
            UserId = currentUser.Id,
            RegisteredAt = DateTime.UtcNow
        };

        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Successfully registered" });
    }

    [HttpDelete("{id}/register")]
    public async Task<IActionResult> CancelRegistration(Guid id)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var registration = await _context.Registrations
            .FirstOrDefaultAsync(r => r.TeeTimeId == id && r.UserId == currentUser.Id);

        if (registration == null)
            return NotFound("Registration not found");

        _context.Registrations.Remove(registration);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/waitlist")]
    public async Task<IActionResult> JoinWaitlist(Guid id)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var teeTime = await _context.TeeTimes.FindAsync(id);
        if (teeTime == null) return NotFound();

        var teeDate = teeTime.TeeDate;

        // All tee times for this day must be full before joining the waitlist
        var dayTeeTimes = await _context.TeeTimes
            .Include(t => t.Registrations)
            .Where(t => t.TeeDate == teeDate)
            .ToListAsync();

        if (!dayTeeTimes.All(t => t.Registrations.Count >= t.MaxPlayers))
            return BadRequest("There are still open spots available today — register for a tee time instead");

        // Must not already be registered on any tee time that day
        var alreadyRegistered = dayTeeTimes.Any(t => t.Registrations.Any(r => r.UserId == currentUser.Id));
        if (alreadyRegistered)
            return BadRequest("You are already registered for a tee time today");

        // Must not already be on the day's waitlist
        var alreadyWaiting = await _context.WaitlistEntries
            .AnyAsync(w => w.TeeDate == teeDate && w.UserId == currentUser.Id);
        if (alreadyWaiting)
            return BadRequest("Already on the waitlist for this day");

        var entry = new WaitlistEntry
        {
            Id = Guid.NewGuid(),
            TeeDate = teeDate,
            UserId = currentUser.Id,
            JoinedAt = DateTime.UtcNow
        };

        _context.WaitlistEntries.Add(entry);
        await _context.SaveChangesAsync();

        var position = await _context.WaitlistEntries
            .Where(w => w.TeeDate == teeDate && w.JoinedAt <= entry.JoinedAt)
            .CountAsync();

        return Ok(new { message = "Successfully joined waitlist", position });
    }

    [HttpDelete("{id}/waitlist")]
    public async Task<IActionResult> LeaveWaitlist(Guid id)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var teeTime = await _context.TeeTimes.FindAsync(id);
        if (teeTime == null) return NotFound();

        var entry = await _context.WaitlistEntries
            .FirstOrDefaultAsync(w => w.TeeDate == teeTime.TeeDate && w.UserId == currentUser.Id);

        if (entry == null)
            return NotFound("Waitlist entry not found");

        _context.WaitlistEntries.Remove(entry);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        return await _userService.GetUserByIdAsync(userId);
    }

    private static UserDto MapToUserDto(User user) => new(
        user.Id,
        user.Email,
        user.FirstName,
        user.LastName,
        user.DisplayName,
        user.Phone,
        user.Handicap,
        user.IsAdmin
    );
}
