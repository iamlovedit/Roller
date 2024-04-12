namespace Roller.Infrastructure.Options;

public class RedisOptions : OptionsBase
{
    public const string Name = "Redis";
    
    public string InstanceName { get; set; }

    public string Host { get; set; }

    public string Password { get; set; }
}