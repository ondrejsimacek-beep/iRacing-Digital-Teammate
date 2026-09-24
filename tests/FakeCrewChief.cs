using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

internal static class FakeCrewChief
{
    private static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "-app_restart")
        {
            Thread.Sleep(30000);
            return;
        }

        string signal = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "restart.flag");
        for (int i = 0; i < 300 && !File.Exists(signal); i++) Thread.Sleep(100);
        if (!File.Exists(signal)) return;
        Process.Start(new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName, "-app_restart")
        {
            UseShellExecute = false
        });
    }
}
