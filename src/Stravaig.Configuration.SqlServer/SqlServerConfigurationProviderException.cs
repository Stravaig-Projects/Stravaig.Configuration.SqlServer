using System;
using System.Runtime.Serialization;

namespace Stravaig.Configuration.SqlServer;

[Serializable]
public class SqlServerConfigurationProviderException : Exception
{
    public SqlServerConfigurationProviderException()
    {
    }

    public SqlServerConfigurationProviderException(string message) : base(message)
    {
    }

    public SqlServerConfigurationProviderException(string message, Exception inner) : base(message, inner)
    {
    }

    protected SqlServerConfigurationProviderException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}