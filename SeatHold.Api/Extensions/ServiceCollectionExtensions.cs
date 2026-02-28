namespace SeatHold.Api.Extensions;

using Microsoft.Extensions.DependencyInjection;
using SeatHold.Api.Middleware;
using SeatHold.Core.Repositories;
using SeatHold.Core.Services;
using SeatHold.Core.Time;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSeatHold(this IServiceCollection services)
    {
        // Core services
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddSingleton<IHoldRepository, InMemoryHoldRepository>();
        services.AddSingleton<IHoldService, HoldService>();

        // Middleware
        services.AddSingleton<ExceptionHandlingMiddleware>();

        return services;
    }
}
