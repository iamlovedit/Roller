namespace Roller.Infrastructure.Options;

public class SqlSugarOptions : OptionsBase
{
    public const string Name = "SqlSugar";

    public SnowFlakeOptions? SnowFlake { get; set; }

    public string Server { get; set; }

    public int? Port { get; set; }

    public string Database { get; set; }

    public string UserId { get; set; }

    public string Password { get; set; }
}

public class SnowFlakeOptions : OptionsBase
{
    public int WorkerId { get; set; }
}