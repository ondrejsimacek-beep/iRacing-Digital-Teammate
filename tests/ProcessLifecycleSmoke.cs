using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DigitalDownforceSimRacing.IRacingTeammate;

internal static class ProcessLifecycleSmoke
{
    private static void Main(string[] args)
    {
        string executable = Path.GetFullPath(args[0]);
        string signal = Path.Combine(Path.GetDirectoryName(executable), "restart.flag");
        if (File.Exists(signal)) File.Delete(signal);
        AppDefinition definition = new AppDefinition
        {
            Key = "crewchief",
            ProcessName = "FakeCrewChief"
        };
        ProcessController controller = new ProcessController();
        int restartsAdopted = 0;
        controller.RestartAdopted += delegate { restartsAdopted++; };
        string error;
        if (!controller.Launch(definition, executable, out error)) throw new Exception(error);
        File.WriteAllText(signal, "restart");

        bool adopted = false;
        for (int i = 0; i < 50; i++)
        {
            Thread.Sleep(200);
            controller.IsRunning(definition);
            if (controller.IsManaged(definition) && HasRestartedProcess())
            {
                adopted = true;
                break;
            }
        }
        if (!adopted) throw new Exception("Crew Chief restart was not adopted.");
        if (restartsAdopted != 1) throw new Exception("Crew Chief restart was not reported once.");
        if (!controller.StopTracked(definition)) throw new Exception("Restarted Crew Chief was not stopped.");
        Thread.Sleep(500);
        if (HasRestartedProcess()) throw new Exception("Restarted Crew Chief is still running.");
        File.Delete(signal);

        using (Process external = Process.Start(executable, "-app_restart"))
        {
            Thread.Sleep(300);
            ProcessController second = new ProcessController();
            if (!second.Launch(definition, executable, out error)) throw new Exception(error);
            if (second.IsManaged(definition)) throw new Exception("Pre-existing process was adopted.");
            if (second.StopTracked(definition)) throw new Exception("Pre-existing process was stopped.");
            if (external.HasExited) throw new Exception("Pre-existing process exited.");
            external.Kill();
            external.WaitForExit();
        }
        SessionJournal journal = new SessionJournal(Path.Combine(Path.GetDirectoryName(executable), "session-journal-smoke"));
        journal.Begin();
        journal.Record("Started Crew Chief");
        if (!journal.Read().Contains("Started Crew Chief")) throw new Exception("Session event was not saved.");
        journal.Begin();
        if (journal.Read().Contains("Started Crew Chief")) throw new Exception("Last session was not replaced.");
        Console.WriteLine("PASS: updater restart tracked; pre-existing process preserved; session journal retained only the last session.");
    }

    private static bool HasRestartedProcess()
    {
        foreach (Process process in Process.GetProcessesByName("FakeCrewChief"))
        using (process)
            try { if (!process.HasExited) return true; } catch { }
        return false;
    }
}
