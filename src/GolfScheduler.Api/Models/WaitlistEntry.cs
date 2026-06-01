using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GolfScheduler.Api.Models;

[Table("waitlist_entries")]
[Index(nameof(TeeDate), nameof(UserId), IsUnique = true)]
public class WaitlistEntry
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("tee_date")]
    public DateOnly TeeDate { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [Column("joined_at")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
