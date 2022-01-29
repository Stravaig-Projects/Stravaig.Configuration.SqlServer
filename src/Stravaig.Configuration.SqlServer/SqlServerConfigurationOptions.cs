namespace Stravaig.Configuration.SqlServer;

public class SqlServerConfigurationOptions
{
    public string? ConnectionString { get; set; }
    public string? ConfigurationSection { get; set; }
    public string SchemaName { get; set; } = "Stravaig";
    public string TableName { get; set; } = "AppConfiguration";
}