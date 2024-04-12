namespace Roller.Infrastructure.Options;

public class AudienceOptions : OptionsBase
{
    public const string Name = "Audience";

    public string Issuer { get; set; }

    public string Audience { get; set; }

    public string Secret { get; set; }


    public int Expiration { get; set; }
}