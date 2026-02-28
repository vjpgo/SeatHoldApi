namespace SeatHold.Core.Contracts;

public sealed class HoldResponse
{
    public Guid Id { get; init; }

    public string SeatId { get; init; } = string.Empty;

    public string HeldBy { get; init; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset ExpiresAtUtc { get; init; }

    public bool IsActive { get; init; }
}
