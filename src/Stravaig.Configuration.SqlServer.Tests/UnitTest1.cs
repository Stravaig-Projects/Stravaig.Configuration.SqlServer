using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;

namespace Stravaig.Configuration.SqlServer.Tests;

public class SqlServerConfigurationProviderTests
{
    private const string DummyConnectionString = "Server=localhost;Database=testing";

    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public void ValuesAreRetrievable()
    {
        var provider = SetupProvider();
        provider.Load();

        provider.TryGet("A", out var value);
        value.ShouldBe("111");
        
        provider.TryGet("B:A", out value);
        value.ShouldBe("222");

        provider.TryGet("B:B", out value);
        value.ShouldBe("333");
    }
    
    private static SqlServerConfigurationProvider SetupProvider()
    {
        var fakeDataLoader = new FakeDataLoader()
        {
            FakeData = new[]
            {
                new KeyValuePair<string, string>("A", "111"),
                new KeyValuePair<string, string>("B:A", "222"),
                new KeyValuePair<string, string>("B:B", "333"),
            }
        };

        var provider =
            new SqlServerConfigurationProvider(new SqlServerConfigurationSource(DummyConnectionString), fakeDataLoader);
        return provider;
    }
}