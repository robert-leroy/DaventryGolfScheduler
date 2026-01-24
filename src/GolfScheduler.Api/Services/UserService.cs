using Microsoft.EntityFrameworkCore;
using GolfScheduler.Api.Data;
using GolfScheduler.Api.Models;

namespace GolfScheduler.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> GetOrCreateUserAsync(string azureAdB2CId, string email, string displayName)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.AzureAdB2CId == azureAdB2CId);

        if (user == null)
        {
            // Parse display name into first/last name
            var nameParts = displayName.Split(' ', 2);
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

            user = new User
            {
                Id = Guid.NewGuid(),
                AzureAdB2CId = azureAdB2CId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        return user;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetUserByAzureIdAsync(string azureAdB2CId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.AzureAdB2CId == azureAdB2CId);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<User?> UpdateAdminStatusAsync(Guid id, bool isAdmin)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        user.IsAdmin = isAdmin;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User?> UpdateUserProfileAsync(Guid id, string firstName, string lastName, string email, string? phone, int? handicap)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;
        user.Phone = phone;
        user.Handicap = handicap;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User> CreateUserAsync(string email, string firstName, string lastName, string? phone, int? handicap, bool isAdmin)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            AzureAdB2CId = $"manual-{Guid.NewGuid()}", // For manually created users
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            Handicap = handicap,
            IsAdmin = isAdmin,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User?> UpdateUserAsync(Guid id, string? email, string? firstName, string? lastName, string? phone, int? handicap, bool? isAdmin)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        if (email != null) user.Email = email;
        if (firstName != null) user.FirstName = firstName;
        if (lastName != null) user.LastName = lastName;
        if (phone != null) user.Phone = phone;
        if (handicap.HasValue) user.Handicap = handicap;
        if (isAdmin.HasValue) user.IsAdmin = isAdmin.Value;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return true;
    }
}
