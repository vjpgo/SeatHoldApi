namespace SeatHold.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeatHold.Core.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddDbContext<SeatHoldDbContext>(
            (serviceProvider, options) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("SeatHoldDb")
                    ?? "Data Source=seathold.db";

                options.UseSqlite(connectionString);
            });
        services.AddScoped<IHoldRepository, SqliteHoldRepository>();

        return services;
    }
}
