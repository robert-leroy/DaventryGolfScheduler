using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using GolfScheduler.Api.Configuration;
using GolfScheduler.Api.Data;
using GolfScheduler.Api.DTOs;
using GolfScheduler.Api.Models;

namespace GolfScheduler.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private const int MaxFailedLoginAttempts = 5;
    private const int LockoutMinutes = 15;

    public AuthService(AppDbContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return new AuthResult { Success = false, Error = "Invalid email or password" };
        }

        // Check if account is locked
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
        {
            var remainingMinutes = (int)(user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes + 1;
            return new AuthResult
            {
                Success = false,
                Error = $"Account is locked. Try again in {remainingMinutes} minute(s)."
            };
        }

        // Check if user has a password set
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return new AuthResult { Success = false, Error = "Password not set. Please contact an administrator." };
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            // Increment failed login count
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= MaxFailedLoginAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                user.FailedLoginCount = 0;
            }
            await _context.SaveChangesAsync();

            return new AuthResult { Success = false, Error = "Invalid email or password" };
        }

        // Successful login - reset failed attempts
        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id);

        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = user
        };
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequestDto request)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return new AuthResult { Success = false, Error = "Email already registered" };
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Handicap = request.Handicap,
            IsAdmin = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id);

        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = user
        };
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (storedToken == null || !storedToken.IsActive)
        {
            return new AuthResult { Success = false, Error = "Invalid or expired refresh token" };
        }

        // Revoke the old refresh token
        storedToken.RevokedAt = DateTime.UtcNow;

        // Generate new tokens
        var accessToken = GenerateAccessToken(storedToken.User);
        var newRefreshToken = await GenerateAndStoreRefreshTokenAsync(storedToken.UserId);

        await _context.SaveChangesAsync();

        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = storedToken.User
        };
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        // Verify current password
        if (string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return false;
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Revoke all refresh tokens for this user (force re-login)
        await RevokeAllUserRefreshTokensAsync(userId);

        return true;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
        if (storedToken != null && storedToken.IsActive)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllUserRefreshTokensAsync(Guid userId)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.DisplayName),
            new Claim("isAdmin", user.IsAdmin.ToString().ToLower()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateAndStoreRefreshTokenAsync(Guid userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var refreshToken = Convert.ToBase64String(randomBytes);

        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();

        return refreshToken;
    }
}
