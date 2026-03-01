namespace SeatHold.Tests.Integration;

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeatHold.Core.Contracts;

[TestClass]
public sealed class HoldsApiTests
{
    private WebApplicationFactory<Program> _factory = default!;
    private HttpClient _client = default!;
    private string _dbPath = string.Empty;

    [TestInitialize]
    public void Init()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"seathold-tests-{Guid.NewGuid():N}.db");

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    (_, config) =>
                    {
                        config.AddInMemoryCollection(
                            new Dictionary<string, string?>
                            {
                                ["ConnectionStrings:SeatHoldDb"] = $"Data Source={_dbPath}"
                            });
                    });
            });

        _client = _factory.CreateClient();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _factory?.Dispose();
        SqliteConnection.ClearAllPools();

        TryDelete(_dbPath);
        TryDelete($"{_dbPath}-wal");
        TryDelete($"{_dbPath}-shm");
    }

    [TestMethod]
    public async Task PostThenGet_ReturnsCreatedThenOk()
    {
        var create = new CreateHoldRequest
        {
            SeatId = "A12",
            HeldBy = "Victor",
            DurationMinutes = 10
        };

        var post = await _client.PostAsJsonAsync("/holds", create);

        Assert.AreEqual(HttpStatusCode.Created, post.StatusCode);

        var created = await post.Content.ReadFromJsonAsync<HoldResponse>();
        Assert.IsNotNull(created);
        Assert.AreNotEqual(Guid.Empty, created!.Id);

        var get = await _client.GetAsync($"/holds/{created.Id}");
        Assert.AreEqual(HttpStatusCode.OK, get.StatusCode);

        var fetched = await get.Content.ReadFromJsonAsync<HoldResponse>();
        Assert.IsNotNull(fetched);
        Assert.AreEqual(created.Id, fetched!.Id);
    }

    [TestMethod]
    public async Task PostTwiceSameSeatWhileActive_ReturnsConflict()
    {
        var request1 = new CreateHoldRequest
        {
            SeatId = "B10",
            HeldBy = "User1",
            DurationMinutes = 30
        };

        var request2 = new CreateHoldRequest
        {
            SeatId = "B10",
            HeldBy = "User2",
            DurationMinutes = 5
        };

        var post1 = await _client.PostAsJsonAsync("/holds", request1);
        var post2 = await _client.PostAsJsonAsync("/holds", request2);

        Assert.AreEqual(HttpStatusCode.Created, post1.StatusCode);
        Assert.AreEqual(HttpStatusCode.Conflict, post2.StatusCode);

        var problem = await post2.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.IsNotNull(problem);
        Assert.AreEqual((int)HttpStatusCode.Conflict, problem!.Status);
    }

    [TestMethod]
    public async Task GetUnknownHold_ReturnsNotFound()
    {
        var get = await _client.GetAsync($"/holds/{Guid.NewGuid()}");
        Assert.AreEqual(HttpStatusCode.NotFound, get.StatusCode);
    }

    private static void TryDelete(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
    }
}
