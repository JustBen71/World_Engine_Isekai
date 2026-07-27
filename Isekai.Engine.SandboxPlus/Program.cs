namespace Isekai.Engine.SandboxPlus;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new SandboxPlusForm());
    }
}
