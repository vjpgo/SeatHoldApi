namespace SeatHold.Core.Contracts;

public sealed class CreateHoldRequest
{
    public string SeatId { get; init; } = string.Empty;

    public string HeldBy { get; init; } = string.Empty;

    public int DurationMinutes { get; init; }
}
