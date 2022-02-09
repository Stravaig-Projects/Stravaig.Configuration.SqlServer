namespace Stravaig.Configuration.SqlServer.Glue;

internal static class DefaultValues
{
    public const int NoRefresh = 0;
    public const int CommandTimeout = 15;
    public const int ConnectionTimeOut = 15;
    public const string SchemaName = "Stravaig";
    public const string TableName = "AppConfiguration";
    public const string ConfigurationSection = "Stravaig:AppConfiguration";
}