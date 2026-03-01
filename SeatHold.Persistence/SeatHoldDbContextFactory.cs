namespace SeatHold.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class SeatHoldDbContextFactory : IDesignTimeDbContextFactory<SeatHoldDbContext>
{
    public SeatHoldDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SeatHoldDbContext>();
        optionsBuilder.UseSqlite("Data Source=seathold.db");

        return new SeatHoldDbContext(optionsBuilder.Options);
    }
}
