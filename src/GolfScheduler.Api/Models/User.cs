using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GolfScheduler.Api.Models;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [MaxLength(255)]
    [Column("password_hash")]
    public string? PasswordHash { get; set; }

    [Column("failed_login_count")]
    public int FailedLoginCount { get; set; } = 0;

    [Column("lockout_end")]
    public DateTime? LockoutEnd { get; set; }

    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }

    [Column("handicap")]
    public int? Handicap { get; set; }

    [Column("is_admin")]
    public bool IsAdmin { get; set; } = false;

    [NotMapped]
    public string DisplayName => $"{FirstName} {LastName}".Trim();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TeeTime> CreatedTeeTimes { get; set; } = new List<TeeTime>();
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
