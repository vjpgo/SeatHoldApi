namespace SeatHold.Core.Services;

using SeatHold.Core.Contracts;

public interface IHoldService
{
    Task<HoldResponse> CreateHoldAsync(CreateHoldRequest request, CancellationToken ct = default);

    Task<HoldResponse?> GetHoldAsync(Guid id, CancellationToken ct = default);

    // Diagnostic endpoint support
    Task<IReadOnlyList<HoldResponse>> GetHoldsAsync(HoldStatusFilter? status, CancellationToken ct = default);
}
