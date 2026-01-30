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
            .Select(t => new TeeTimeListDto(
                t.Id,
                t.TeeDate,
                t.TeeTimeValue,
                t.MaxPlayers,
                t.Notes,
                t.Registrations.Count,
                t.MaxPlayers - t.Registrations.Count,
                t.Registrations.Any(r => r.UserId == currentUser.Id)
            ))
            .ToListAsync();

        return Ok(teeTimes);
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
        var teeTime = await _context.TeeTimes
            .Include(t => t.CreatedBy)
            .Include(t => t.Registrations)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teeTime == null) return NotFound();

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
            teeTime.MaxPlayers - teeTime.Registrations.Count
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
            teeTime.MaxPlayers
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
            teeTime.MaxPlayers - teeTime.Registrations.Count
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
