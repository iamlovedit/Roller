namespace Roller.Infrastructure.Options;

public class MongoDbOptions : OptionsBase
{
    public const string Name = "MongoDb";

    public string? ConnectionString { get; set; }

    public string? Database { get; set; }
}