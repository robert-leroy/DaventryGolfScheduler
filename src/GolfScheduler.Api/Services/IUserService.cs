using GolfScheduler.Api.Models;

namespace GolfScheduler.Api.Services;

public interface IUserService
{
    Task<User> GetOrCreateUserAsync(string azureAdB2CId, string email, string displayName);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User?> GetUserByAzureIdAsync(string azureAdB2CId);
    Task<List<User>> GetAllUsersAsync();
    Task<User?> UpdateAdminStatusAsync(Guid id, bool isAdmin);
    Task<User?> UpdateUserProfileAsync(Guid id, string firstName, string lastName, string email, string? phone, int? handicap);
    Task<User> CreateUserAsync(string email, string firstName, string lastName, string? phone, int? handicap, bool isAdmin);
    Task<User?> UpdateUserAsync(Guid id, string? email, string? firstName, string? lastName, string? phone, int? handicap, bool? isAdmin);
    Task<bool> DeleteUserAsync(Guid id);
}
