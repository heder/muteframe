using System.Runtime.Versioning;

namespace MuteFrame;

internal static class Program
{
    [STAThread]
    [SupportedOSPlatform("windows")]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        _ = new App();

        Application.Run();
    }
}