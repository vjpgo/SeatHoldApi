namespace SeatHold.Tests.Unit;

using SeatHold.Tests; // for TestAssert
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeatHold.Core.Contracts;
using SeatHold.Core.Exceptions;
using SeatHold.Core.Repositories;
using SeatHold.Core.Services;

[TestClass]
public sealed class HoldServiceTests
{
    private static readonly DateTimeOffset FixedNowUtc =
        new(year: 2026, month: 2, day: 24, hour: 12, minute: 0, second: 0, offset: TimeSpan.Zero);

    [TestMethod]
    public async Task CreateHoldAsync_ValidRequest_CreatesHoldAndComputesExpiry()
    {
        // Arrange
        var repo = new InMemoryHoldRepository();
        var clock = new FakeClock(FixedNowUtc);
        var service = new HoldService(repo, clock);

        var request = new CreateHoldRequest
        {
            SeatId = "A12",
            HeldBy = "Victor",
            DurationMinutes = 15
        };

        // Act
        var created = await service.CreateHoldAsync(request);

        // Assert
        Assert.AreNotEqual(Guid.Empty, created.Id);
        Assert.AreEqual("A12", created.SeatId);
        Assert.AreEqual("Victor", created.HeldBy);
        Assert.AreEqual(FixedNowUtc, created.CreatedAtUtc);
        Assert.AreEqual(FixedNowUtc.AddMinutes(15), created.ExpiresAtUtc);
        Assert.IsTrue(created.IsActive);
    }

[TestMethod]
public async Task CreateHoldAsync_DurationMinutesNotGreaterThanZero_ThrowsInvalidHoldRequestException()
{
    // Arrange
    var repo = new InMemoryHoldRepository();
    var clock = new FakeClock(FixedNowUtc);
    var service = new HoldService(repo, clock);

    var invalidDurations = new[] { 0, -1, -30 };

    foreach (var durationMinutes in invalidDurations)
    {
        var request = new CreateHoldRequest
        {
            SeatId = "A12",
            HeldBy = "Victor",
            DurationMinutes = durationMinutes
        };

        // Act + Assert
        _ = await TestAssert.ThrowsAsync<InvalidHoldRequestException>(
            () => service.CreateHoldAsync(request));
    }
}

[TestMethod]
public async Task CreateHoldAsync_MissingRequiredFields_ThrowsInvalidHoldRequestException()
{
    // Arrange
    var cases = new (string SeatId, string HeldBy, int Duration, string Expected)[]
    {
        ("", "Victor", 10, "SeatId is required."),
        ("   ", "Victor", 10, "SeatId is required."),
        ("A12", "", 10, "HeldBy is required."),
        ("A12", "   ", 10, "HeldBy is required."),
    };

    foreach (var c in cases)
    {
        var repo = new InMemoryHoldRepository();
        var clock = new FakeClock(FixedNowUtc);
        var service = new HoldService(repo, clock);

        var request = new CreateHoldRequest
        {
            SeatId = c.SeatId,
            HeldBy = c.HeldBy,
            DurationMinutes = c.Duration
        };

        // Act
        var ex = await TestAssert.ThrowsAsync<InvalidHoldRequestException>(
            () => service.CreateHoldAsync(request));

        // Assert
        Assert.AreEqual(c.Expected, ex.Message);
    }
}

    [TestMethod]
    public async Task CreateHoldAsync_WhenSeatAlreadyHasActiveHold_ThrowsSeatAlreadyHeldException()
    {
        // Arrange
        var repo = new InMemoryHoldRepository();
        var clock = new FakeClock(FixedNowUtc);
        var service = new HoldService(repo, clock);

        var request1 = new CreateHoldRequest
        {
            SeatId = "A12",
            HeldBy = "Victor",
            DurationMinutes = 30
        };

        var request2 = new CreateHoldRequest
        {
            SeatId = "A12",
            HeldBy = "SomeoneElse",
            DurationMinutes = 5
        };

        // Act
        _ = await service.CreateHoldAsync(request1);

        // Assert
        await TestAssert.ThrowsAsync<SeatAlreadyHeldException>(
            () => service.CreateHoldAsync(request2));
    }

    [TestMethod]
    public async Task CreateHoldAsync_WhenExistingHoldExpired_AllowsNewHold()
    {
        // Arrange
        var repo = new InMemoryHoldRepository();
        var clock = new FakeClock(FixedNowUtc);
        var service = new HoldService(repo, clock);

        var request1 = new CreateHoldRequest
        {
            SeatId = "A12",
            HeldBy = "Victor",
            DurationMinutes = 10
        };

        var request2 = new CreateHoldRequest
        {
            SeatId = "A12",
            HeldBy = "SomeoneElse",
            DurationMinutes = 5
        };

        // Act
        var first = await service.CreateHoldAsync(request1);

        // Advance time past expiry
        clock.UtcNow = first.ExpiresAtUtc.AddSeconds(1);

        var second = await service.CreateHoldAsync(request2);

        // Assert
        Assert.AreNotEqual(first.Id, second.Id);
        Assert.AreEqual("A12", second.SeatId);
        Assert.AreEqual("SomeoneElse", second.HeldBy);
        Assert.IsTrue(second.IsActive);
    }

    [TestMethod]
    public async Task GetHoldAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var repo = new InMemoryHoldRepository();
        var clock = new FakeClock(FixedNowUtc);
        var service = new HoldService(repo, clock);

        // Act
        var result = await service.GetHoldAsync(Guid.NewGuid());

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetHoldAsync_WhenFound_ReturnsHoldResponseWithIsActiveComputed()
    {
        // Arrange
        var repo = new InMemoryHoldRepository();
        var clock = new FakeClock(FixedNowUtc);
        var service = new HoldService(repo, clock);

        var created = await service.CreateHoldAsync(new CreateHoldRequest
        {
            SeatId = "A12",
            HeldBy = "Victor",
            DurationMinutes = 1
        });

        // Act + Assert (active)
        var fetchedActive = await service.GetHoldAsync(created.Id);
        Assert.IsNotNull(fetchedActive);
        Assert.IsTrue(fetchedActive!.IsActive);

        // Advance time to expire
        clock.UtcNow = created.ExpiresAtUtc;

        var fetchedExpired = await service.GetHoldAsync(created.Id);
        Assert.IsNotNull(fetchedExpired);
        Assert.IsFalse(fetchedExpired!.IsActive);
    }

    [TestMethod]
    public async Task GetHoldsAsync_FilterActive_ReturnsOnlyActive()
    {
        // Arrange
        var repo = new InMemoryHoldRepository();
        var clock = new FakeClock(FixedNowUtc);
        var service = new HoldService(repo, clock);

        var active = await service.CreateHoldAsync(new CreateHoldRequest
        {
            SeatId = "A1",
            HeldBy = "User1",
            DurationMinutes = 60
        });

        var expiringSoon = await service.CreateHoldAsync(new CreateHoldRequest
        {
            SeatId = "A2",
            HeldBy = "User2",
            DurationMinutes = 1
        });

        // Expire A2
        clock.UtcNow = expiringSoon.ExpiresAtUtc;

        // Act
        var activeOnly = await service.GetHoldsAsync(HoldStatusFilter.Active);

        // Assert
        Assert.IsTrue(activeOnly.Any(h => h.Id == active.Id));
        Assert.IsFalse(activeOnly.Any(h => h.Id == expiringSoon.Id));
    }
}
