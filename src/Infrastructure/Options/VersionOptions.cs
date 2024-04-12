namespace Roller.Infrastructure.Options;

public class VersionOptions : OptionsBase
{
    public const string Name = "Version";
    public string HeaderName { get; set; }

    public string ParameterName { get; set; }

    public string SwaggerTitle { get; set; }
}