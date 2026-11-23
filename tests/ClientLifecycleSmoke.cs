using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DigitalDownforceSimRacing.IRacingTeammate;

internal static class ClientLifecycleSmoke
{
    internal static void Run(string fixture)
    {
        var state = new AutoModeLifecycle();
        Expect(state, false, true, false, AutoModeAction.None, "Opening only the client");
        Expect(state, true, true, true, AutoModeAction.None, "Simulator appears while launcher is busy");
        Expect(state, true, true, false, AutoModeAction.StartStack, "Busy launcher becomes free");

        var app = new AppDefinition { Key = "client-lifetime-smoke", ProcessName = "FakeExternal" };
        var controller = new ProcessController();
        string error;
        try
        {
            if (!controller.Launch(app, Path.GetFullPath(fixture), out error)) throw new Exception(error);
            Expect(state, false, true, false, AutoModeAction.KeepRunning, "First simulator session ends");
            if (!controller.IsManaged(app) || !controller.IsRunning(app))
                throw new Exception("Companion ownership was lost between simulator sessions.");
            Expect(state, false, true, false, AutoModeAction.None, "Client remains open between sessions");
            Expect(state, true, true, false, AutoModeAction.StartStack, "Second simulator session starts");
            if (!controller.Launch(app, fixture, out error)) throw new Exception(error);
            Process[] copies = Process.GetProcessesByName(app.ProcessName);
            try { if (copies.Length != 1) throw new Exception("Second session duplicated the companion app."); }
            finally { foreach (Process copy in copies) copy.Dispose(); }
            Expect(state, true, false, false, AutoModeAction.None, "Client closes while simulator still runs");
            if (AutoModeLifecycle.CanCleanup(true, false)) throw new Exception("Cleanup was allowed during a simulator session.");
            Expect(state, false, false, false, AutoModeAction.StopStack, "Both simulator and client close");
            if (!state.StackActive) throw new Exception("Ownership scope was cleared before cleanup confirmation.");
            if (AutoModeLifecycle.CanCleanup(false, true)) throw new Exception("Reopened client did not cancel cleanup.");
            Expect(state, false, true, false, AutoModeAction.None, "Client reopens during confirmation");
            Expect(state, false, false, false, AutoModeAction.StopStack, "Client closes again");
            if (!controller.StopTracked(app)) throw new Exception("Managed companion was not stopped on client close.");
            state.CompleteStop();
            Expect(state, false, false, false, AutoModeAction.None, "Cleanup completed");
            for (int i = 0; i < 25 && controller.IsRunning(app); i++) Thread.Sleep(200);
            if (controller.IsRunning(app)) throw new Exception("Companion is still running after client cleanup.");
        }
        finally { controller.StopTracked(app); }

        Expect(state, false, true, false, AutoModeAction.None, "New client without a simulator");
        Expect(state, true, true, false, AutoModeAction.StartStack, "First session in a new client cycle");
        if (state.Poll(false, false, false, false) != AutoModeAction.None)
            throw new Exception("Disabled Auto Mode requested cleanup.");
        Expect(state, false, false, true, AutoModeAction.None, "Cleanup waits while another operation runs");
        Expect(state, false, false, false, AutoModeAction.StopStack, "Cleanup resumes after the operation");
        Console.WriteLine("PASS: apps survive simulator changes and stop only after client close; busy and restart transitions preserved.");
    }

    private static void Expect(AutoModeLifecycle state, bool simulator, bool client, bool busy,
        AutoModeAction expected, string scenario)
    {
        AutoModeAction actual = state.Poll(true, simulator, client, busy);
        if (actual != expected) throw new Exception(scenario + ": expected " + expected + ", got " + actual);
    }
}
