using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GolfScheduler.Api.Models;

[Table("registrations")]
[Index(nameof(TeeTimeId), nameof(UserId), IsUnique = true)]
public class Registration
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("tee_time_id")]
    public Guid TeeTimeId { get; set; }

    [ForeignKey("TeeTimeId")]
    public TeeTime TeeTime { get; set; } = null!;

    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [Column("registered_at")]
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
