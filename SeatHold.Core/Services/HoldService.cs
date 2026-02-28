namespace SeatHold.Core.Services;

using System.Collections.Concurrent;
using SeatHold.Core.Contracts;
using SeatHold.Core.Exceptions;
using SeatHold.Core.Models;
using SeatHold.Core.Repositories;
using SeatHold.Core.Time;

public sealed class HoldService : IHoldService
{
    private readonly IHoldRepository _repo;
    private readonly ISystemClock _clock;

    // One lock per seatId to enforce atomic "check active then create"
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _seatLocks =
        new(StringComparer.OrdinalIgnoreCase);

    public HoldService(IHoldRepository repo, ISystemClock clock)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<HoldResponse> CreateHoldAsync(CreateHoldRequest request, CancellationToken ct = default)
    {
        ValidateCreateRequest(request);

        var seatKey = request.SeatId.Trim();
        var seatLock = _seatLocks.GetOrAdd(seatKey, _ => new SemaphoreSlim(1, 1));

        await seatLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var nowUtc = _clock.UtcNow;

            var existing = await _repo.GetActiveBySeatAsync(seatKey, nowUtc, ct).ConfigureAwait(false);
            if (existing is not null)
            {
                throw new SeatAlreadyHeldException(seatKey);
            }

            var hold = new Hold
            {
                Id = Guid.NewGuid(),
                SeatId = seatKey,
                HeldBy = request.HeldBy.Trim(),
                CreatedAtUtc = nowUtc,
                ExpiresAtUtc = nowUtc.AddMinutes(request.DurationMinutes)
            };

            await _repo.CreateAsync(hold, ct).ConfigureAwait(false);

            return MapToResponse(hold, nowUtc);
        }
        finally
        {
            seatLock.Release();
        }
    }

    public async Task<HoldResponse?> GetHoldAsync(Guid id, CancellationToken ct = default)
    {
        var hold = await _repo.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (hold is null)
        {
            return null;
        }

        var nowUtc = _clock.UtcNow;
        return MapToResponse(hold, nowUtc);
    }

    public async Task<IReadOnlyList<HoldResponse>> GetHoldsAsync(HoldStatusFilter? status, CancellationToken ct = default)
    {
        var holds = await _repo.GetAllAsync(ct).ConfigureAwait(false);
        var nowUtc = _clock.UtcNow;

        IEnumerable<Hold> filtered = holds;

        if (status is HoldStatusFilter.Active)
        {
            filtered = filtered.Where(h => h.ExpiresAtUtc > nowUtc);
        }
        else if (status is HoldStatusFilter.Expired)
        {
            filtered = filtered.Where(h => h.ExpiresAtUtc <= nowUtc);
        }

        return filtered
            .Select(h => MapToResponse(h, nowUtc))
            .ToList();
    }

    private static void ValidateCreateRequest(CreateHoldRequest request)
    {
        if (request is null)
        {
            throw new InvalidHoldRequestException("Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(request.SeatId))
        {
            throw new InvalidHoldRequestException("SeatId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.HeldBy))
        {
            throw new InvalidHoldRequestException("HeldBy is required.");
        }

        if (request.DurationMinutes <= 0)
        {
            throw new InvalidHoldRequestException("DurationMinutes must be greater than zero.");
        }
    }

    private static HoldResponse MapToResponse(Hold hold, DateTimeOffset nowUtc)
    {
        return new HoldResponse
        {
            Id = hold.Id,
            SeatId = hold.SeatId,
            HeldBy = hold.HeldBy,
            CreatedAtUtc = hold.CreatedAtUtc,
            ExpiresAtUtc = hold.ExpiresAtUtc,
            IsActive = hold.ExpiresAtUtc > nowUtc
        };
    }
}
