using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shouldly;

namespace Stravaig.Configuration.SqlServer.Tests;

[TestFixture]
public class SourceBuilderTests
{
    [Test]
    public void DeveloperIsBeingObtuseByDeliberatelyNullingSchema_ThrowsException()
    {
        // Arrange
        const string theConnectionString = "Server=MyServer;Database=MyDatabase";
        var options = (SqlServerConfigurationOptions opts) =>
        {
            opts.ConnectionString = theConnectionString;
            opts.SchemaName = null!;
        };
        var configBuilder = SetupConfig();

        // Act & Assert
        var ex = Should.Throw<SqlServerConfigurationProviderException>(
            () => SourceBuilder.BuildSource(configBuilder, options));

        ex.Message.ShouldBe("The schema name is required to use SQL Server Configuration.");
    }

    [Test]
    public void DeveloperIsBeingObtuseByDeliberatelyNullingTable_ThrowsException()
    {
        // Arrange
        const string theConnectionString = "Server=MyServer;Database=MyDatabase";
        var options = (SqlServerConfigurationOptions opts) =>
        {
            opts.ConnectionString = theConnectionString;
            opts.TableName = null!;
        };
        var configBuilder = SetupConfig();

        // Act & Assert
        var ex = Should.Throw<SqlServerConfigurationProviderException>(
            () => SourceBuilder.BuildSource(configBuilder, options));

        ex.Message.ShouldBe("The table name is required to use SQL Server Configuration.");
    }

    [Test]
    public void SimplestOptions_HappyPath()
    {
        // Arrange
        const string theConnectionString = "Server=MyServer;Database=MyDatabase";
        var options = (SqlServerConfigurationOptions opts) =>
        {
            opts.ConnectionString = theConnectionString;
        };
        var configBuilder = SetupConfig();

        // Act
        var source = SourceBuilder.BuildSource(configBuilder, options);

        // Assert
        source.ConnectionString.ShouldBe(theConnectionString);
        source.RefreshInterval.ShouldBe(TimeSpan.FromSeconds(DefaultValues.NoRefresh));
        source.SchemaName.ShouldBe(DefaultValues.SchemaName);
        source.TableName.ShouldBe(DefaultValues.TableName);
    }

    [Test]
    public void FillAllOptionsManually_HappyPath()
    {
        // Arrange
        const string theConnectionString = "Server=MyServer;Database=MyDatabase";
        const int refreshSeconds = 60;
        const string schemaName = "MyConfigInfo";
        const string tableName = "AppConfig";
        var options = (SqlServerConfigurationOptions opts) =>
        {
            opts.ConnectionString = theConnectionString;
            opts.RefreshSeconds = refreshSeconds;
            opts.SchemaName = schemaName;
            opts.TableName = tableName;
        };
        var configBuilder = SetupConfig();

        // Act
        var source = SourceBuilder.BuildSource(configBuilder, options);

        // Assert
        source.ConnectionString.ShouldBe(theConnectionString);
        source.RefreshInterval.ShouldBe(TimeSpan.FromSeconds(refreshSeconds));
        source.SchemaName.ShouldBe(schemaName);
        source.TableName.ShouldBe(tableName);
    }

    [Test]
    public void NoConnectionString_ThrowsException()
    {
        // Arrange
        var configBuilder = SetupConfig();

        // Act & Assert
        var ex = Should.Throw<SqlServerConfigurationProviderException>( 
            () => SourceBuilder.BuildSource(configBuilder, null));
        ex.Message.ShouldBe("Cannot build a SQL Server Configuration Provider without a connection string.");
    }

    [Test]
    public void FillAllOptionsFromExistingConfig_HappyPath()
    {
        // Arrange
        const string theConnectionString = "Server=MyServer;Database=MyDatabase";
        const int refreshSeconds = 60;
        const string schemaName = "MyConfigInfo";
        const string tableName = "AppConfig";
        var options = (SqlServerConfigurationOptions opts) =>
        {
            opts.ConfigurationSection = "SqlConfiguration";
        };
        var configBuilder = SetupConfig(builder =>
        {
            builder.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("SqlConfiguration:ConnectionString", theConnectionString),
                new KeyValuePair<string, string>("SqlConfiguration:RefreshSeconds", refreshSeconds.ToString()),
                new KeyValuePair<string, string>("SqlConfiguration:SchemaName", schemaName),
                new KeyValuePair<string, string>("SqlConfiguration:TableName", tableName),
            });
        });

        // Act
        var source = SourceBuilder.BuildSource(configBuilder, options);

        // Assert
        source.ConnectionString.ShouldBe(theConnectionString);
        source.RefreshInterval.ShouldBe(TimeSpan.FromSeconds(refreshSeconds));
        source.SchemaName.ShouldBe(schemaName);
        source.TableName.ShouldBe(tableName);
    }

    [Test]
    public void FillAllOptionsFromExistingConfigWithNamedConnectionString_HappyPath()
    {
        // Arrange
        const string theConnectionString = "Server=MyServer;Database=MyDatabase";
        const int refreshSeconds = 60;
        const string schemaName = "MyConfigInfo";
        const string tableName = "AppConfig";
        var options = (SqlServerConfigurationOptions opts) =>
        {
            opts.ConfigurationSection = "SqlConfiguration";
        };
        var configBuilder = SetupConfig(builder =>
        {
            builder.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:ConfigDB", theConnectionString),
                new KeyValuePair<string, string>("SqlConfiguration:RefreshSeconds", refreshSeconds.ToString()),
                new KeyValuePair<string, string>("SqlConfiguration:SchemaName", schemaName),
                new KeyValuePair<string, string>("SqlConfiguration:TableName", tableName),
                new KeyValuePair<string, string>("SqlConfiguration:ConnectionStringName", "ConfigDB")
            });
        });

        // Act
        var source = SourceBuilder.BuildSource(configBuilder, options);

        // Assert
        source.ConnectionString.ShouldBe(theConnectionString);
        source.RefreshInterval.ShouldBe(TimeSpan.FromSeconds(refreshSeconds));
        source.SchemaName.ShouldBe(schemaName);
        source.TableName.ShouldBe(tableName);
    }

    [Test]
    public void TwoCompetingConnectionStrings_DirectOneWins()
    {
        // Arrange
        const string theConnectionString = "Server=MyServer;Database=MyDatabase";
        const string theLosingConnectionString = "Server=TheLosingServer;Database=TheLosingDatabase";
        const int refreshSeconds = 60;
        const string schemaName = "MyConfigInfo";
        const string tableName = "AppConfig";
        var options = (SqlServerConfigurationOptions opts) =>
        {
            opts.ConfigurationSection = "SqlConfiguration";
        };
        var configBuilder = SetupConfig(builder =>
        {
            builder.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:ConfigDB", theLosingConnectionString),
                new KeyValuePair<string, string>("SqlConfiguration:ConnectionString", theConnectionString),
                new KeyValuePair<string, string>("SqlConfiguration:RefreshSeconds", refreshSeconds.ToString()),
                new KeyValuePair<string, string>("SqlConfiguration:SchemaName", schemaName),
                new KeyValuePair<string, string>("SqlConfiguration:TableName", tableName),
                new KeyValuePair<string, string>("SqlConfiguration:ConnectionStringName", "ConfigDB")
            });
        });

        // Act
        var source = SourceBuilder.BuildSource(configBuilder, options);

        // Assert
        source.ConnectionString.ShouldBe(theConnectionString);
        source.RefreshInterval.ShouldBe(TimeSpan.FromSeconds(refreshSeconds));
        source.SchemaName.ShouldBe(schemaName);
        source.TableName.ShouldBe(tableName);
    }

    private IConfigurationBuilder SetupConfig(Action<IConfigurationBuilder>? configure = null)
    {
        var builder = new ConfigurationBuilder();
        configure?.Invoke(builder);
        return builder;
    }
}