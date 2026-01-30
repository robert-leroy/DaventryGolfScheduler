using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GolfScheduler.Api.Models;

[Table("refresh_tokens")]
public class RefreshToken
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("token")]
    public string Token { get; set; } = string.Empty;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    public User User { get; set; } = null!;

    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    [NotMapped]
    public bool IsRevoked => RevokedAt != null;

    [NotMapped]
    public bool IsActive => !IsExpired && !IsRevoked;
}
