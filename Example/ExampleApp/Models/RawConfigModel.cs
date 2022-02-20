namespace ExampleApp.Models;

public class RawConfigModel
{
    public string[] Providers { get; init; }
    public KeyValuePair<string, string>[] Values { get; init; }
}