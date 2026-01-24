using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GolfScheduler.Api.DTOs;
using GolfScheduler.Api.Models;
using GolfScheduler.Api.Services;
using System.Security.Claims;

namespace GolfScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        return Ok(MapToProfileDto(user));
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateCurrentUserProfile([FromBody] UserProfileUpdateDto dto)
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        var updated = await _userService.UpdateUserProfileAsync(user.Id, dto.FirstName, dto.LastName, dto.Email, dto.Phone, dto.Handicap);
        if (updated == null) return NotFound();

        return Ok(MapToProfileDto(updated));
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        var dtos = users.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(MapToDto(user));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserCreateDto dto)
    {
        var user = await _userService.CreateUserAsync(dto.Email, dto.FirstName, dto.LastName, dto.Phone, dto.Handicap, dto.IsAdmin);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, MapToDto(user));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UserUpdateDto dto)
    {
        var user = await _userService.UpdateUserAsync(id, dto.Email, dto.FirstName, dto.LastName, dto.Phone, dto.Handicap, dto.IsAdmin);
        if (user == null) return NotFound();
        return Ok(MapToDto(user));
    }

    [HttpPut("{id}/admin")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserDto>> UpdateAdminStatus(Guid id, [FromBody] UserAdminUpdateDto dto)
    {
        var user = await _userService.UpdateAdminStatusAsync(id, dto.IsAdmin);
        if (user == null) return NotFound();
        return Ok(MapToDto(user));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser == null) return Unauthorized();

        if (currentUser.Id == id)
            return BadRequest("You cannot delete your own account");

        var result = await _userService.DeleteUserAsync(id);
        if (!result) return NotFound();

        return NoContent();
    }

    private static UserDto MapToDto(User user) => new(
        user.Id,
        user.Email,
        user.FirstName,
        user.LastName,
        user.DisplayName,
        user.Phone,
        user.Handicap,
        user.IsAdmin
    );

    private static UserProfileDto MapToProfileDto(User user) => new(
        user.Id,
        user.Email,
        user.FirstName,
        user.LastName,
        user.DisplayName,
        user.Phone,
        user.Handicap,
        user.IsAdmin,
        user.CreatedAt
    );

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
