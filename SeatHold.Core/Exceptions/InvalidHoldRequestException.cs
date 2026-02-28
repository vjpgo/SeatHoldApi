namespace SeatHold.Core.Exceptions;

public sealed class InvalidHoldRequestException : Exception
{
    public InvalidHoldRequestException(string message) : base(message) { }
}
