namespace SeatHold.Core.Repositories;

using System.Collections.Concurrent;
using SeatHold.Core.Models;

public sealed class InMemoryHoldRepository : IHoldRepository
{
    private readonly ConcurrentDictionary<Guid, Hold> _holdsById = new();

    public Task CreateAsync(Hold hold, CancellationToken ct = default)
    {
        // Last-write-wins is fine for this in-memory store. Service enforces uniqueness rules.
        _holdsById[hold.Id] = hold;
        return Task.CompletedTask;
    }

    public Task<Hold?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _holdsById.TryGetValue(id, out var hold);
        return Task.FromResult(hold);
    }

    public Task<Hold?> GetActiveBySeatAsync(string seatId, DateTimeOffset nowUtc, CancellationToken ct = default)
    {
        // For small in-memory store, linear scan is acceptable and clear.
        // If we later need performance, we can add a secondary index by seatId.
        foreach (var hold in _holdsById.Values)
        {
            if (string.Equals(hold.SeatId, seatId, StringComparison.OrdinalIgnoreCase) &&
                hold.ExpiresAtUtc > nowUtc)
            {
                return Task.FromResult<Hold?>(hold);
            }
        }

        return Task.FromResult<Hold?>(null);
    }

    public Task<IReadOnlyList<Hold>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Hold> snapshot = _holdsById.Values.ToList();
        return Task.FromResult(snapshot);
    }
}
