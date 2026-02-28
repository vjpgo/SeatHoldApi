namespace SeatHold.Core.Repositories;

using SeatHold.Core.Models;

public interface IHoldRepository
{
    Task<Hold?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Hold?> GetActiveBySeatAsync(
        string seatId,
        DateTimeOffset nowUtc,
        CancellationToken ct = default);

    Task CreateAsync(Hold hold, CancellationToken ct = default);

    // Diagnostic endpoint support
    Task<IReadOnlyList<Hold>> GetAllAsync(CancellationToken ct = default);
}
