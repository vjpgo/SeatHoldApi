namespace SeatHold.Tests.Unit;

using SeatHold.Core.Time;

internal sealed class FakeClock : ISystemClock
{
    public FakeClock(DateTimeOffset utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTimeOffset UtcNow { get; set; }
}
