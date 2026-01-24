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
public class RegistrationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;

    public RegistrationsController(AppDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<List<UserRegistrationDto>>> GetMyRegistrations()
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var registrations = await _context.Registrations
            .Include(r => r.TeeTime)
            .Where(r => r.UserId == currentUser.Id && r.TeeTime.TeeDate >= today)
            .OrderBy(r => r.TeeTime.TeeDate)
            .ThenBy(r => r.TeeTime.TeeTimeValue)
            .Select(r => new UserRegistrationDto(
                r.Id,
                r.TeeTimeId,
                r.TeeTime.TeeDate,
                r.TeeTime.TeeTimeValue,
                r.RegisteredAt
            ))
            .ToListAsync();

        return Ok(registrations);
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        var azureId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(azureId)) return null;

        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("emails")
            ?? "";
        var name = User.FindFirstValue("name")
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? "Unknown";

        return await _userService.GetOrCreateUserAsync(azureId, email, name);
    }
}
