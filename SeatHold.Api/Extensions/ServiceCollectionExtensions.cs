namespace SeatHold.Api.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeatHold.Api.Middleware;
using SeatHold.Core.Repositories;
using SeatHold.Core.Services;
using SeatHold.Core.Time;
using SeatHold.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSeatHold(this IServiceCollection services, IConfiguration config)
    {
        // Core services
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddScoped<IHoldService, HoldService>();

        // Middleware
        services.AddSingleton<ExceptionHandlingMiddleware>();

        // Persistence selection (simple, interview-friendly)
        var provider = config["Persistence:Provider"] ?? "Sqlite";

        if (provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IHoldRepository, InMemoryHoldRepository>();
        }
        else
        {
            services.AddPersistence();
        }

        return services;
    }
}
