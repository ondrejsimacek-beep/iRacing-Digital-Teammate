using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DigitalDownforceSimRacing.IRacingTeammate;

internal static class ProcessLifecycleSmoke
{
    private static void Main(string[] args)
    {
        AppDefinition coach = AppCatalog.Create().Find(x => x.Key == "ddscoach");
        string coachPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs", "DDS Telemetry Coach", "DDS Telemetry Coach.exe");
        if (coach == null || coach.Name != "DDS Telemetry Coach" || coach.ProcessName != "DDS Telemetry Coach" ||
            !coach.EnabledByDefault || coach.CandidatePaths.Length != 1 || coach.CandidatePaths[0] != coachPath)
            throw new Exception("DDS Coach catalog entry does not match its installer and process name.");
        if (File.Exists(coachPath) && AppCatalog.DetectPath(coach) != coachPath)
            throw new Exception("Installed DDS Coach was not detected.");
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
        for (int i = 0; i < 100; i++)
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

        if (new AppSetting().StopExternalAfterSession) throw new Exception("External cleanup must default to off.");
        AppDefinition handoff = new AppDefinition { Key = "handoff", ProcessName = "FakeExternal" };
        ProcessController handoffController = new ProcessController();
        if (!handoffController.Launch(handoff, Path.GetFullPath(args[2]), out error)) throw new Exception(error);
        if (!handoffController.IsManaged(handoff)) throw new Exception("Differently named handoff target was not tracked.");
        if (!handoffController.StopTracked(handoff)) throw new Exception("Differently named handoff target was not stopped.");
        Thread.Sleep(300);
        if (Process.GetProcessesByName("FakeExternal").Length != 0)
            throw new Exception("Differently named handoff target is still running.");

        using (Process external = Process.Start(Path.GetFullPath(args[1]), "-app_restart"))
        {
            Thread.Sleep(300);
            AppDefinition portableCoach = new AppDefinition
            {
                Key = "ddscoach", ProcessName = "FakeExternal", CandidatePaths = new string[0]
            };
            if (AppCatalog.DetectPath(portableCoach) != Path.GetFullPath(args[1]))
                throw new Exception("Running portable DDS Coach was not detected.");
            AppDefinition unrelated = new AppDefinition { Key = "garage61", ProcessName = "FakeExternal" };
            if (ProcessController.StopExternalAfterSession(unrelated, out error))
                throw new Exception("External cleanup accepted an unrelated app.");
            if (external.HasExited) throw new Exception("Unrelated external app was stopped.");
            AppDefinition optedIn = new AppDefinition { Key = "irdashies", ProcessName = "FakeExternal" };
            if (!ProcessController.StopExternalAfterSession(optedIn, out error)) throw new Exception(error);
            if (!external.WaitForExit(5000)) throw new Exception("Opted-in external app was not stopped.");
        }
        Console.WriteLine("PASS: restart ownership, handoff cleanup, external opt-in safety and last-session journal.");
    }

    private static bool HasRestartedProcess()
    {
        foreach (Process process in Process.GetProcessesByName("FakeCrewChief"))
        using (process)
            try { if (!process.HasExited) return true; } catch { }
        return false;
    }
}
