namespace SeatHold.Persistence;

using Microsoft.EntityFrameworkCore;
using SeatHold.Core.Exceptions;
using SeatHold.Core.Models;
using SeatHold.Core.Repositories;
using SeatHold.Core.Time;

public sealed class SqliteHoldRepository : IHoldRepository
{
    private readonly SeatHoldDbContext _dbContext;
    private readonly ISystemClock _clock;

    public SqliteHoldRepository(SeatHoldDbContext dbContext, ISystemClock clock)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task CreateAsync(Hold hold, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(hold);

        var seatKey = hold.SeatId.Trim();
        var nowUtc = _clock.UtcNow.UtcDateTime;

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);

        var hasActiveHold = await _dbContext.Holds
            .AsNoTracking()
            .AnyAsync(x => x.SeatId == seatKey && x.ExpiresAtUtc > nowUtc, ct);

        if (hasActiveHold)
        {
            throw new SeatAlreadyHeldException(seatKey);
        }

        // Normalize SeatId at persistence boundary (Hold.SeatId is init-only)
        var entity = MapToEntity(hold);
        entity.SeatId = seatKey;

        _dbContext.Holds.Add(entity);
        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }


    public async Task<Hold?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _dbContext.Holds
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, ct)
            .ConfigureAwait(false);

        return entity is null ? null : MapToModel(entity);
    }

    public async Task<Hold?> GetActiveBySeatAsync(
        string seatId,
        DateTimeOffset nowUtc,
        CancellationToken ct = default)
    {
        var seatKey = seatId.Trim();
        var now = nowUtc.UtcDateTime;

        var entity = await _dbContext.Holds
            .AsNoTracking()
            .Where(x => x.SeatId == seatKey && x.ExpiresAtUtc > now)
            .OrderByDescending(x => x.ExpiresAtUtc)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        return entity is null ? null : MapToModel(entity);
    }

    public async Task<IReadOnlyList<Hold>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _dbContext.Holds
            .AsNoTracking()
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return entities.Select(MapToModel).ToList();
    }

    private static HoldEntity MapToEntity(Hold hold)
    {
        return new HoldEntity
        {
            Id = hold.Id,
            SeatId = hold.SeatId,
            HeldBy = hold.HeldBy,
            CreatedAtUtc = hold.CreatedAtUtc.UtcDateTime,
            ExpiresAtUtc = hold.ExpiresAtUtc.UtcDateTime
        };
    }

    private static Hold MapToModel(HoldEntity entity)
    {
        return new Hold
        {
            Id = entity.Id,
            SeatId = entity.SeatId,
            HeldBy = entity.HeldBy,
            CreatedAtUtc = new DateTimeOffset(DateTime.SpecifyKind(entity.CreatedAtUtc, DateTimeKind.Utc)),
            ExpiresAtUtc = new DateTimeOffset(DateTime.SpecifyKind(entity.ExpiresAtUtc, DateTimeKind.Utc))
        };
    }
}
