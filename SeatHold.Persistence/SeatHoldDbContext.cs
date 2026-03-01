namespace SeatHold.Persistence;

using Microsoft.EntityFrameworkCore;

public sealed class SeatHoldDbContext : DbContext
{
    public SeatHoldDbContext(DbContextOptions<SeatHoldDbContext> options)
        : base(options)
    {
    }

    public DbSet<HoldEntity> Holds => Set<HoldEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var hold = modelBuilder.Entity<HoldEntity>();

        hold.ToTable("Holds");
        hold.HasKey(x => x.Id);

        hold.Property(x => x.SeatId)
            .IsRequired()
            .UseCollation("NOCASE")
            .HasMaxLength(64);

        hold.Property(x => x.HeldBy)
            .IsRequired()
            .HasMaxLength(256);

        hold.Property(x => x.CreatedAtUtc)
            .IsRequired();

        hold.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        hold.HasIndex(x => x.SeatId);
        hold.HasIndex(x => new { x.SeatId, x.ExpiresAtUtc });
    }
}
