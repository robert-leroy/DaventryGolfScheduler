using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GolfScheduler.Api.DTOs;
using GolfScheduler.Api.Services;

namespace GolfScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new AuthResponseDto(
            result.AccessToken!,
            result.RefreshToken!,
            result.ExpiresAt!.Value,
            ToUserProfileDto(result.User!)
        ));
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(new AuthResponseDto(
            result.AccessToken!,
            result.RefreshToken!,
            result.ExpiresAt!.Value,
            ToUserProfileDto(result.User!)
        ));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.Error });
        }

        return Ok(new AuthResponseDto(
            result.AccessToken!,
            result.RefreshToken!,
            result.ExpiresAt!.Value,
            ToUserProfileDto(result.User!)
        ));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
    {
        await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
        return Ok(new { message = "Logged out successfully" });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var success = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

        if (!success)
        {
            return BadRequest(new { message = "Current password is incorrect" });
        }

        return Ok(new { message = "Password changed successfully" });
    }

    private static UserProfileDto ToUserProfileDto(GolfScheduler.Api.Models.User user)
    {
        return new UserProfileDto(
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
    }
}
