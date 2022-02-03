using System;

namespace Stravaig.Configuration.SqlServer;

public class SqlServerConfigurationOptions
{
    internal bool IsLoggerExpected { get; private set; }
    public string? ConnectionString { get; set; }
    
    public string? ConnectionStringName { get; set; }
    public string? ConfigurationSection { get; set; }
    public string SchemaName { get; set; } = DefaultValues.SchemaName;
    public string TableName { get; set; } = DefaultValues.TableName;

    public int RefreshSeconds { get; set; } = DefaultValues.NoRefresh;
    
    public SqlServerConfigurationOptions FromExistingConfiguration(string configurationSection = DefaultValues.ConfigurationSection)
    {
        ConfigurationSection = configurationSection ?? throw new ArgumentNullException(nameof(configurationSection));
        ConnectionString = null;
        return this;
    }

    public SqlServerConfigurationOptions ExpectLogger()
    {
        IsLoggerExpected = true;
        return this;
    }
}