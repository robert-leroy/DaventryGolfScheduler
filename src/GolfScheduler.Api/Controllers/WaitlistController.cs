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
public class WaitlistController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;

    public WaitlistController(AppDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<UserWaitlistEntryDto>>> GetMyWaitlist()
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var entries = await _context.WaitlistEntries
            .Where(w => w.UserId == currentUser.Id && w.TeeDate >= today)
            .OrderBy(w => w.TeeDate)
            .ToListAsync();

        var result = new List<UserWaitlistEntryDto>();
        foreach (var entry in entries)
        {
            var position = await _context.WaitlistEntries
                .Where(w => w.TeeDate == entry.TeeDate && w.JoinedAt <= entry.JoinedAt)
                .CountAsync();

            result.Add(new UserWaitlistEntryDto(
                entry.Id,
                entry.TeeDate,
                entry.JoinedAt,
                position
            ));
        }

        return Ok(result);
    }

    [HttpDelete("day/{date}")]
    public async Task<IActionResult> LeaveWaitlistByDate(DateOnly date)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var entry = await _context.WaitlistEntries
            .FirstOrDefaultAsync(w => w.TeeDate == date && w.UserId == currentUser.Id);

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
}
