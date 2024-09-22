namespace Roller.Infrastructure.Options;

public class CrossOptions : OptionsBase
{
    public const string Name = "Cros";

    public bool AllowAnyMethod { get; set; }

    public string[] Methods { get; set; }

    public bool AllowAnyHeader { get; set; }

    public string[] Headers { get; set; }

    public bool AllowAnyOrigin { get; set; }

    public string[] Origins { get; set; }
}