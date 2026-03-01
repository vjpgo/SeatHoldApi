namespace SeatHold.Persistence;

public sealed class HoldEntity
{
    public Guid Id { get; set; }

    public string SeatId { get; set; } = string.Empty;

    public string HeldBy { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }
}
