namespace Roller.Infrastructure.SetupExtensions;

public static class EncryptionSetup
{
    public static IServiceCollection AddAesEncryption(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IAesEncryptionService, AesEncryptionService>();
        return services;
    }
}