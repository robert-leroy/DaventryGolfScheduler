using GolfScheduler.Api.DTOs;
using GolfScheduler.Api.Models;

namespace GolfScheduler.Api.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(RegisterRequestDto request);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task RevokeAllUserRefreshTokensAsync(Guid userId);
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public User? User { get; set; }
    public string? Error { get; set; }
}
