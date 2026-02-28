namespace SeatHold.Core.Exceptions;

public sealed class SeatAlreadyHeldException : Exception
{
    public SeatAlreadyHeldException(string seatId)
        : base($"Seat '{seatId}' already has an active hold.")
    {
        SeatId = seatId;
    }

    public string SeatId { get; }
}
