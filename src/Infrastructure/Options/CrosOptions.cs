namespace Roller.Infrastructure.Options;

public class CrossOptions : OptionsBase
{
    public const string Name = "Cros";

    public string PoliyName { get; set; }

    public bool AllowAnyMethod { get; set; }

    public bool AllowAnyHeader { get; set; }

    public bool AllowAnyOrigin { get; set; }
}