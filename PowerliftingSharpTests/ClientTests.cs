using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerliftingSharp;
using PowerliftingSharp.Types;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerliftingSharpTests;

#pragma warning disable CS8618
[TestClass]
public class ClientTests
{
    private static PLClient _client;

    [ClassInitialize]
    public static void ClassInit(TestContext context) => _client = new();

    [ClassCleanup]
    public static void ClassCleanup() => _client.Dispose();

    [TestMethod]
    public async Task NameQueryTest()
    {
        string nameToQuery = "Andrey Malanichev";

        (string FoundName, string Identifier)? retval = await _client.QueryName(nameToQuery);

        Assert.IsNotNull(retval);
        Assert.AreEqual("andreymalanichev", retval.Value.Identifier);
        Assert.AreEqual(nameToQuery, retval.Value.FoundName);
    }

    [TestMethod]
    public void BadQueryTest()
    {
        Parallel.For(0, 15, async i =>
        {
            Random rng = new();
            byte[] bytes = new byte[512];
            rng.NextBytes(bytes);

            string randomString = Encoding.Unicode.GetString(bytes);

            Assert.IsNull(await _client.QueryName(randomString));
        });
    }

    [TestMethod]
    public async Task GetLifterTest()
    {
        Athlete? andrey = await _client.GetAthleteByIdentifierAsync("andreymalanichev");

        Assert.IsNotNull(andrey);
    }

    [TestMethod]
    public async Task GetBadLifterTest()
    {
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => _client.GetAthleteByIdentifierAsync("foobar"));
    }

    [TestMethod]
    public async Task CancellationTokenTest()
    {
        CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _client.GetAthleteByIdentifierAsync("andreymalanichev", cts.Token));
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _client.QueryName("Andrey", cts.Token));
    }
}
#pragma warning restore CS8618