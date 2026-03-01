using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using SeatHold.Api.Extensions;
using SeatHold.Api.Middleware;
using SeatHold.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSeatHold();

var app = builder.Build();

var connectionString = app.Configuration.GetConnectionString("SeatHoldDb")
    ?? "Data Source=seathold.db";

DatabaseInitializer.Migrate(app.Services, connectionString);

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();

// Needed for WebApplicationFactory integration tests
public partial class Program { }

internal static class DatabaseInitializer
{
    private static readonly ConcurrentDictionary<string, object> Locks =
        new(StringComparer.OrdinalIgnoreCase);

    public static void Migrate(IServiceProvider services, string connectionString)
    {
        var syncRoot = Locks.GetOrAdd(connectionString, _ => new object());

        lock (syncRoot)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SeatHoldDbContext>();
            db.Database.Migrate();
        }
    }
}