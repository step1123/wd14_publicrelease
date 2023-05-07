using Robust.Shared.Configuration;

namespace Content.Server.White;

public static class UnsafePseudoIoC // Я НЕНАВИЖУ IOCMAANGERRESOLVEPOSHEL NAHUI
{
    public static IConfigurationManager ConfigurationManager = default!;

    public static void Initialize()
    {
        ConfigurationManager = IoCManager.Resolve<IConfigurationManager>();
    }

}
