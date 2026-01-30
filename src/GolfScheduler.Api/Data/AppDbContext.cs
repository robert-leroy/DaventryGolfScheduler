using Microsoft.EntityFrameworkCore;
using GolfScheduler.Api.Models;

namespace GolfScheduler.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<TeeTime> TeeTimes { get; set; } = null!;
    public DbSet<Registration> Registrations { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Token).IsUnique();
        });

        modelBuilder.Entity<TeeTime>(entity =>
        {
            entity.HasOne(t => t.CreatedBy)
                .WithMany(u => u.CreatedTeeTimes)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.TeeDate, e.TeeTimeValue });
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasOne(r => r.TeeTime)
                .WithMany(t => t.Registrations)
                .HasForeignKey(r => r.TeeTimeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.User)
                .WithMany(u => u.Registrations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
