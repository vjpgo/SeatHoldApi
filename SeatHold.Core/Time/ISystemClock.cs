namespace SeatHold.Core.Time;

public interface ISystemClock
{
    DateTimeOffset UtcNow { get; }
}
