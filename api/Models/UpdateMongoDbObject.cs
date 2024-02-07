namespace mongodbweb.Server.Models;

public class UpdateMongoDbObject
{
    public Dictionary<string, object>? Differences { get; init; }
    public Dictionary<string, string>? RenameMap { get; init; }
}