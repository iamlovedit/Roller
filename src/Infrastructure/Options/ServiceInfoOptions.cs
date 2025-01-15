namespace Roller.Infrastructure.Options;

public class ServiceInfoOptions : OptionsBase
{
    public const string Name = "ServiceInfo";

    public string? Version { get; set; }

    public string? Description { get; set; }
    
    public string? ServiceName { get; set; }
}