namespace SeatHold.Api.Extensions;

using Microsoft.Extensions.DependencyInjection;
using SeatHold.Api.Middleware;
using SeatHold.Core.Services;
using SeatHold.Core.Time;
using SeatHold.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSeatHold(this IServiceCollection services)
    {
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddPersistence();
        services.AddScoped<IHoldService, HoldService>();

        services.AddSingleton<ExceptionHandlingMiddleware>();

        return services;
    }
}
