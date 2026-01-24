using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GolfScheduler.Api.Models;

[Table("tee_times")]
public class TeeTime
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("tee_date")]
    public DateOnly TeeDate { get; set; }

    [Required]
    [Column("tee_time")]
    public TimeOnly TeeTimeValue { get; set; }

    [Column("max_players")]
    public int MaxPlayers { get; set; } = 4;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_by")]
    public Guid CreatedById { get; set; }

    [ForeignKey("CreatedById")]
    public User CreatedBy { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
