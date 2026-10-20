using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

internal static class FakeHandoffLauncher
{
    private static void Main(string[] args)
    {
        string target = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FakeExternal.exe");
        Process.Start(new ProcessStartInfo(target, "-app_restart") { UseShellExecute = false });
        Thread.Sleep(500);
    }
}
