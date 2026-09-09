using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace DigitalDownforceSimRacing.IRacingTeammate
{
    [Serializable]
    public class LauncherSettings
    {
        public List<AppSetting> Apps { get; set; }
        public int DefaultDelaySeconds { get; set; }
        public string UpdateRepository { get; set; }
        public bool AutoModeEnabled { get; set; }
        public DateTime LastAutomaticUpdateCheckUtc { get; set; }

        public LauncherSettings()
        {
            Apps = new List<AppSetting>();
            DefaultDelaySeconds = 2;
            UpdateRepository = "";
            AutoModeEnabled = true;
            LastAutomaticUpdateCheckUtc = DateTime.MinValue;
        }
    }

    [Serializable]
    public class AppSetting
    {
        public string Key { get; set; }
        public string Path { get; set; }
        public bool Enabled { get; set; }
        public int DelaySeconds { get; set; }
        public bool Hidden { get; set; }

        public AppSetting()
        {
            Key = "";
            Path = "";
            Enabled = true;
            DelaySeconds = 2;
            Hidden = false;
        }
    }

    public class AppDefinition
    {
        public string Key;
        public string Name;
        public string Category;
        public string ProcessName;
        public string Initials;
        public bool EnabledByDefault;
        public bool RequiresElevationBridge;
        public string[] CandidatePaths;
    }

    public static class AppCatalog
    {
        public static List<AppDefinition> Create()
        {
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pfx86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            return new List<AppDefinition>
            {
                Def("iracing", "iRacing", "SIMULATOR", "iRacingUI", "iR", true,
                    Path.Combine(pfx86, "iRacing", "iRacingLauncher64.exe"),
                    Path.Combine(pfx86, "iRacing", "iRacingUI.exe"),
                    Path.Combine(local, "Programs", "iRacing UI", "iRacingUI.exe")),
                Def("crewchief", "Crew Chief V4", "SPOTTER", "CrewChiefV4", "CC", true,
                    Path.Combine(pfx86, "Britton IT Ltd", "CrewChiefV4", "CrewChiefV4.exe"),
                    Path.Combine(pf, "Britton IT Ltd", "CrewChiefV4", "CrewChiefV4.exe")),
                Def("tradingpaints", "Trading Paints", "LIVERIES", "Trading Paints", "TP", true,
                    Path.Combine(pfx86, "Rhinode LLC", "Trading Paints", "Trading Paints.exe"),
                    Path.Combine(pf, "Rhinode LLC", "Trading Paints", "Trading Paints.exe")),
                Def("garage61", "Garage61", "TELEMETRY", "garage61-launcher", "G61", true,
                    Path.Combine(local, "Garage61 Agent", "garage61-launcher.exe"),
                    Path.Combine(local, "Programs", "Garage61", "garage61-launcher.exe"),
                    Path.Combine(roaming, "Garage61", "garage61-launcher.exe"),
                    Path.Combine(roaming, "garage61-install", "garage61-launcher.exe")),
                Def("irdashies", "irDashies", "OPEN OVERLAYS", "irdashies", "iD", false,
                    Path.Combine(local, "irdashies", "irdashies.exe"),
                    Path.Combine(local, "Programs", "irdashies", "irdashies.exe")),
                Def("edgeoverlays", "Edge Overlays", "OVERLAY", "Edge Overlays", "EO", false,
                    Path.Combine(pf, "Edge Overlays", "Edge Overlays.exe"),
                    Path.Combine(pfx86, "Edge Overlays", "Edge Overlays.exe"),
                    Path.Combine(local, "Programs", "Edge Overlays", "Edge Overlays.exe"),
                    Path.Combine(local, "Edge Overlays", "Edge Overlays.exe")),
                Def("gofast", "GO Fast", "GO SETUPS", "GO Fast", "GO", false,
                    Path.Combine(local, "Programs", "GO Fast", "GO Fast.exe"),
                    Path.Combine(local, "Programs", "go-fast", "GO Fast.exe"),
                    Path.Combine(local, "GO Fast", "GO Fast.exe"),
                    Path.Combine(local, "gosetups", "GO Fast.exe")),
                Def("simhub", "SimHub", "DASHBOARDS", "SimHubWPF", "SH", false,
                    Path.Combine(pfx86, "SimHub", "SimHubWPF.exe"),
                    Path.Combine(pf, "SimHub", "SimHubWPF.exe")),
                Def("ioverlay", "iOverlay", "OVERLAY", "iOverlay", "iO", false,
                    Path.Combine(local, "Programs", "iOverlay", "iOverlay.exe"),
                    Path.Combine(local, "iOverlay", "iOverlay.exe")),
                Def("racelab", "Racelab", "OVERLAY", "RacelabApps", "RL", false,
                    Path.Combine(local, "Programs", "RacelabApps", "RacelabApps.exe"),
                    Path.Combine(local, "Programs", "racelabapps", "RacelabApps.exe")),
                Def("streamdeck", "Elgato Stream Deck", "CONTROLS", "StreamDeck", "SD", false,
                    Path.Combine(pf, "Elgato", "StreamDeck", "StreamDeck.exe")),
                ElevatedDef("conspit", "CONSPIT Launcher", "HARDWARE", "ConspitLink2.0", "CL", true,
                    Path.Combine(pfx86, "Conspit Link 2.0", "ConspitLink2.0.exe"),
                    Path.Combine(pf, "Conspit Link 2.0", "ConspitLink2.0.exe")),
                Def("simconnectmanager", "SimConnect Manager", "HARDWARE", "SimConnectManager", "SC", true,
                    Path.Combine(pf, "Simnet Racing", "SimConnectManager.exe"),
                    Path.Combine(pfx86, "Simnet Racing", "SimConnectManager.exe")),
                Def("mozapithouse", "MOZA Pit House", "HARDWARE", "MOZA Pit House", "MZ", false,
                    Path.Combine(pfx86, "MOZA Pit House", "MOZA Pit House.exe"),
                    Path.Combine(pf, "MOZA Pit House", "MOZA Pit House.exe")),
                Def("irsidekick", "iRSidekick", "PAINTS & TOOLS", "iRSidekick", "iRS", false,
                    Path.Combine(local, "Programs", "iRSidekick", "iRSidekick.exe"),
                    Path.Combine(local, "iRSidekick", "iRSidekick.exe"),
                    Path.Combine(pf, "iRSidekick", "iRSidekick.exe")),
                Def("vrslogger", "VRS Telemetry Logger", "TELEMETRY", "VRS_TelemetryLogger", "VRS", false,
                    Path.Combine(pfx86, "Virtual Racing School", "VRS Telemetry Logger", "VRS_TelemetryLogger.exe"),
                    Path.Combine(pf, "Virtual Racing School", "VRS Telemetry Logger", "VRS_TelemetryLogger.exe"),
                    Path.Combine(local, "Programs", "VRS Telemetry Logger", "VRS_TelemetryLogger.exe")),
                Def("kapps", "Kapps", "OVERLAY", "Kapps", "KA", false,
                    Path.Combine(local, "Programs", "Kapps", "Kapps.exe"),
                    Path.Combine(local, "Kapps", "Kapps.exe")),
                Def("jrt", "Joel Real Timing", "TIMING", "JRT", "JRT", false,
                    Path.Combine(pf, "Joel Real Timing", "JRT.exe"),
                    Path.Combine(pfx86, "Joel Real Timing", "JRT.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Joel Real Timing", "JRT.exe")),
                Def("openkneeboard", "OpenKneeboard", "VR TOOL", "OpenKneeboardApp", "OKB", false,
                    Path.Combine(local, "Programs", "OpenKneeboard", "OpenKneeboardApp.exe"),
                    Path.Combine(pf, "OpenKneeboard", "OpenKneeboardApp.exe")),
                Def("maira", "Marvin's AIRA", "RACE ENGINEER", "MarvinsAIRARefactored", "MA", false,
                    Path.Combine(pf, "Marvins Awesome iRacing App - Refactored", "MarvinsAIRARefactored.exe"),
                    Path.Combine(pfx86, "Marvins Awesome iRacing App - Refactored", "MarvinsAIRARefactored.exe"))
            };
        }

        private static AppDefinition Def(string key, string name, string category, string processName,
            string initials, bool enabled, params string[] paths)
        {
            return new AppDefinition
            {
                Key = key,
                Name = name,
                Category = category,
                ProcessName = processName,
                Initials = initials,
                EnabledByDefault = enabled,
                CandidatePaths = paths.Where(delegate(string value) { return !String.IsNullOrWhiteSpace(value); }).ToArray()
            };
        }

        private static AppDefinition ElevatedDef(string key, string name, string category, string processName,
            string initials, bool enabled, params string[] paths)
        {
            AppDefinition definition = Def(key, name, category, processName, initials, enabled, paths);
            definition.RequiresElevationBridge = true;
            return definition;
        }

        public static string DetectPath(AppDefinition definition)
        {
            if (definition.Key == "iracing")
            {
                string iracing = DetectIRacing();
                if (!String.IsNullOrWhiteSpace(iracing)) return iracing;
            }
            foreach (string candidate in definition.CandidatePaths)
            {
                try
                {
                    string expanded = Environment.ExpandEnvironmentVariables(candidate);
                    if (File.Exists(expanded)) return Path.GetFullPath(expanded);
                }
                catch { }
            }
            return "";
        }

        private static string DetectIRacing()
        {
            string install = FindInstalledLocation("iRacing.com Race Simulation");
            if (!String.IsNullOrWhiteSpace(install))
            {
                string[] registryCandidates =
                {
                    Path.Combine(install, "ui", "iRacingUI.exe"),
                    Path.Combine(install, "iRacingLauncher64.exe"),
                    Path.Combine(install, "iRacingUI.exe")
                };
                foreach (string candidate in registryCandidates)
                    if (File.Exists(candidate)) return candidate;
            }

            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (!drive.IsReady || drive.DriveType != DriveType.Fixed) continue;
                    string root = drive.RootDirectory.FullName;
                    string[] candidates =
                    {
                        Path.Combine(root, "Games", "iRacing", "ui", "iRacingUI.exe"),
                        Path.Combine(root, "Games", "iRacing", "iRacingLauncher64.exe"),
                        Path.Combine(root, "iRacing", "ui", "iRacingUI.exe"),
                        Path.Combine(root, "iRacing", "iRacingLauncher64.exe"),
                        Path.Combine(root, "SteamLibrary", "steamapps", "common", "iRacing", "ui", "iRacingUI.exe")
                    };
                    foreach (string candidate in candidates)
                        if (File.Exists(candidate)) return candidate;
                }
            }
            catch { }
            return "";
        }

        private static string FindInstalledLocation(string displayName)
        {
            RegistryKey[] roots = { Registry.LocalMachine, Registry.CurrentUser };
            string[] subkeys =
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };
            foreach (RegistryKey root in roots)
            {
                foreach (string subkey in subkeys)
                {
                    try
                    {
                        using (RegistryKey uninstall = root.OpenSubKey(subkey))
                        {
                            if (uninstall == null) continue;
                            foreach (string name in uninstall.GetSubKeyNames())
                            {
                                using (RegistryKey item = uninstall.OpenSubKey(name))
                                {
                                    if (item == null) continue;
                                    string title = item.GetValue("DisplayName") as string;
                                    if (!String.Equals(title, displayName, StringComparison.OrdinalIgnoreCase)) continue;
                                    string location = item.GetValue("InstallLocation") as string;
                                    if (!String.IsNullOrWhiteSpace(location)) return location.Trim('"');
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            return "";
        }
    }

    public class SettingsStore
    {
        private readonly string directory;
        private readonly string filePath;
        private readonly string legacyFilePath;

        public string DirectoryPath { get { return directory; } }

        public SettingsStore()
        {
            directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DDS",
                "iRacing Digital Teammate");
            filePath = Path.Combine(directory, "settings.xml");
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string previousDdsPath = Path.Combine(roaming, "DDS", "iRacing Teammate", "settings.xml");
            string legacyBrandDirectory = "Sna" + "ils Motorsport";
            string originalPath = Path.Combine(roaming, legacyBrandDirectory, "iRacing Teammate", "settings.xml");
            legacyFilePath = File.Exists(previousDdsPath) ? previousDdsPath : originalPath;
        }

        public LauncherSettings Load(List<AppDefinition> definitions)
        {
            LauncherSettings settings = null;
            try
            {
                string sourceFile = File.Exists(filePath) ? filePath : legacyFilePath;
                if (File.Exists(sourceFile))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(LauncherSettings));
                    using (FileStream stream = File.OpenRead(sourceFile))
                        settings = (LauncherSettings)serializer.Deserialize(stream);
                }
            }
            catch { settings = null; }

            if (settings == null) settings = new LauncherSettings();
            HashSet<string> supportedKeys = new HashSet<string>(definitions.Select(delegate(AppDefinition item) { return item.Key; }));
            settings.Apps.RemoveAll(delegate(AppSetting item) { return !supportedKeys.Contains(item.Key); });
            foreach (AppDefinition definition in definitions)
            {
                AppSetting item = settings.Apps.FirstOrDefault(delegate(AppSetting app) { return app.Key == definition.Key; });
                if (item == null)
                {
                    item = new AppSetting
                    {
                        Key = definition.Key,
                        Enabled = definition.EnabledByDefault,
                        DelaySeconds = settings.DefaultDelaySeconds,
                        Path = AppCatalog.DetectPath(definition)
                    };
                    settings.Apps.Add(item);
                }
                else if (String.IsNullOrWhiteSpace(item.Path) || !File.Exists(item.Path))
                {
                    string detected = AppCatalog.DetectPath(definition);
                    if (!String.IsNullOrWhiteSpace(detected)) item.Path = detected;
                }
            }
            Save(settings);
            return settings;
        }

        public void Save(LauncherSettings settings)
        {
            try
            {
                Directory.CreateDirectory(directory);
                string temp = filePath + ".tmp";
                XmlSerializer serializer = new XmlSerializer(typeof(LauncherSettings));
                using (FileStream stream = File.Create(temp)) serializer.Serialize(stream, settings);
                if (File.Exists(filePath)) File.Delete(filePath);
                File.Move(temp, filePath);
            }
            catch { }
        }
    }

    public class ProcessController
    {
        private readonly Dictionary<string, List<Process>> tracked = new Dictionary<string, List<Process>>();
        private readonly HashSet<string> elevatedTaskLaunches = new HashSet<string>();
        private readonly object sync = new object();

        public static bool IsIRacingSessionRunning()
        {
            string[] sessionProcesses =
            {
                "iRacingSim64DX11",
                "iRacingSim64DX12",
                "iRacingSim64"
            };
            foreach (string processName in sessionProcesses)
            {
                try
                {
                    Process[] matches = Process.GetProcessesByName(processName);
                    bool running = matches.Length > 0;
                    foreach (Process process in matches) process.Dispose();
                    if (running) return true;
                }
                catch { }
            }
            return false;
        }

        public bool IsRunning(AppDefinition definition)
        {
            lock (sync)
            {
                List<Process> processes;
                if (tracked.TryGetValue(definition.Key, out processes))
                {
                    for (int i = processes.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            if (!processes[i].HasExited) return true;
                        }
                        catch { }
                        try { processes[i].Dispose(); } catch { }
                        processes.RemoveAt(i);
                    }
                    tracked.Remove(definition.Key);
                }
            }
            try
            {
                Process[] matches = Process.GetProcessesByName(definition.ProcessName);
                bool running = matches.Length > 0;
                foreach (Process match in matches) match.Dispose();
                return running;
            }
            catch { return false; }
        }

        public bool Launch(AppDefinition definition, string path, out string error)
        {
            error = "";
            if (IsRunning(definition)) return true;
            if (String.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                error = "Executable was not found.";
                return false;
            }
            try
            {
                HashSet<int> existingProcessIds = SnapshotProcessIds(definition.ProcessName);
                if (definition.RequiresElevationBridge)
                {
                    if (!ElevatedTaskBridge.Launch(definition.Key, path, out error)) return false;

                    List<Process> elevatedProcesses = new List<Process>();
                    int stableElevatedScans = 0;
                    for (int scan = 0; scan < 25 && stableElevatedScans < 3; scan++)
                    {
                        Thread.Sleep(200);
                        bool added = AddNewProcesses(definition.ProcessName, existingProcessIds, elevatedProcesses);
                        stableElevatedScans = elevatedProcesses.Count > 0 && !added ? stableElevatedScans + 1 : 0;
                    }
                    if (elevatedProcesses.Count == 0)
                    {
                        error = "The elevated application did not start.";
                        return false;
                    }
                    lock (sync)
                    {
                        tracked[definition.Key] = elevatedProcesses;
                        elevatedTaskLaunches.Add(definition.Key);
                    }
                    return true;
                }

                ProcessStartInfo info = new ProcessStartInfo(path);
                info.WorkingDirectory = Path.GetDirectoryName(path);
                info.UseShellExecute = true;
                Process process = Process.Start(info);
                if (process == null)
                {
                    error = "Windows did not return a process handle.";
                    return false;
                }

                List<Process> launchedProcesses = new List<Process>();
                launchedProcesses.Add(process);
                int stableScans = 0;
                for (int scan = 0; scan < 15 && stableScans < 3; scan++)
                {
                    Thread.Sleep(200);
                    bool added = AddNewProcesses(definition.ProcessName, existingProcessIds, launchedProcesses);
                    stableScans = launchedProcesses.Count > 1 && !added ? stableScans + 1 : 0;
                }

                lock (sync) tracked[definition.Key] = launchedProcesses;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool StopTracked(AppDefinition definition)
        {
            List<Process> launchedProcesses = null;
            bool elevatedTaskLaunch = false;
            lock (sync)
            {
                if (tracked.ContainsKey(definition.Key))
                {
                    launchedProcesses = tracked[definition.Key];
                    tracked.Remove(definition.Key);
                }
                elevatedTaskLaunch = elevatedTaskLaunches.Remove(definition.Key);
            }
            if (launchedProcesses == null && !elevatedTaskLaunch) return false;

            if (elevatedTaskLaunch)
            {
                bool stopped = ElevatedTaskBridge.Stop(definition.Key);
                if (launchedProcesses != null)
                    foreach (Process process in launchedProcesses)
                        try { process.Dispose(); } catch { }
                return stopped;
            }

            bool foundRunningProcess = false;
            foreach (Process process in launchedProcesses.GroupBy(delegate(Process item)
            {
                try { return item.Id; } catch { return -1; }
            }).Select(delegate(IGrouping<int, Process> group) { return group.First(); }))
            {
                try
                {
                    if (!process.HasExited)
                    {
                        foundRunningProcess = true;
                        ProcessStartInfo taskkill = new ProcessStartInfo("taskkill.exe",
                            "/PID " + process.Id + " /T /F");
                        taskkill.CreateNoWindow = true;
                        taskkill.UseShellExecute = false;
                        Process killer = Process.Start(taskkill);
                        if (killer != null)
                        {
                            killer.WaitForExit(5000);
                            killer.Dispose();
                        }
                    }
                }
                catch
                {
                    try { if (!process.HasExited) process.Kill(); } catch { }
                }
                try { process.Dispose(); } catch { }
            }
            return foundRunningProcess;
        }

        private static HashSet<int> SnapshotProcessIds(string processName)
        {
            HashSet<int> ids = new HashSet<int>();
            try
            {
                Process[] matches = Process.GetProcessesByName(processName);
                foreach (Process match in matches)
                {
                    try { ids.Add(match.Id); } catch { }
                    match.Dispose();
                }
            }
            catch { }
            return ids;
        }

        private static bool AddNewProcesses(string processName, HashSet<int> existingProcessIds,
            List<Process> launchedProcesses)
        {
            bool added = false;
            HashSet<int> trackedIds = new HashSet<int>();
            foreach (Process trackedProcess in launchedProcesses)
            {
                try { trackedIds.Add(trackedProcess.Id); } catch { }
            }

            try
            {
                Process[] matches = Process.GetProcessesByName(processName);
                foreach (Process match in matches)
                {
                    int id;
                    try { id = match.Id; }
                    catch
                    {
                        match.Dispose();
                        continue;
                    }
                    if (!existingProcessIds.Contains(id) && !trackedIds.Contains(id))
                    {
                        launchedProcesses.Add(match);
                        trackedIds.Add(id);
                        added = true;
                    }
                    else match.Dispose();
                }
            }
            catch { }
            return added;
        }
    }

    public static class ElevatedTaskBridge
    {
        private const string ConspitKey = "conspit";
        private const string ConspitTaskName = "DDS iRacing Digital Teammate - CONSPIT Launcher";

        public static bool Launch(string key, string path, out string error)
        {
            error = "";
            if (!IsAllowedTarget(key, path))
            {
                error = "CONSPIT admin bridge only allows ConspitLink2.0.exe installed under Program Files.";
                return false;
            }

            if (!IsConfigured(key, path))
            {
                try
                {
                    ProcessStartInfo setup = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location);
                    setup.Arguments = "--configure-elevated-task " + key + " \"" + path + "\"";
                    setup.Verb = "runas";
                    setup.UseShellExecute = true;
                    Process process = Process.Start(setup);
                    if (process == null)
                    {
                        error = "Windows did not start the one-time admin bridge setup.";
                        return false;
                    }
                    process.WaitForExit();
                    int exitCode = process.ExitCode;
                    process.Dispose();
                    if (exitCode != 0 || !IsConfigured(key, path))
                    {
                        error = "The one-time CONSPIT admin bridge setup was not completed.";
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    error = "The one-time CONSPIT admin bridge setup was cancelled or failed: " + ex.Message;
                    return false;
                }
            }

            int result = RunSchtasks("/Run /TN \"" + TaskName(key) + "\"", out error);
            if (result != 0)
            {
                if (String.IsNullOrWhiteSpace(error)) error = "Task Scheduler could not start CONSPIT.";
                return false;
            }
            return true;
        }

        public static bool Stop(string key)
        {
            string output;
            return RunSchtasks("/End /TN \"" + TaskName(key) + "\"", out output) == 0;
        }

        public static int Configure(string key, string path)
        {
            if (!IsAllowedTarget(key, path)) return 10;

            string xmlPath = Path.Combine(Path.GetTempPath(), "dds-conspit-task-" + Guid.NewGuid().ToString("N") + ".xml");
            try
            {
                string sid = WindowsIdentity.GetCurrent().User.Value;
                string command = SecurityElement.Escape(Path.GetFullPath(path));
                string workingDirectory = SecurityElement.Escape(Path.GetDirectoryName(Path.GetFullPath(path)));
                string xml = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n" +
                    "<Task version=\"1.2\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\">" +
                    "<RegistrationInfo><Author>Digital Downforce Sim Racing</Author>" +
                    "<Description>Starts and stops CONSPIT Launcher with an iRacing session.</Description></RegistrationInfo>" +
                    "<Principals><Principal id=\"Author\"><UserId>" + SecurityElement.Escape(sid) + "</UserId>" +
                    "<LogonType>InteractiveToken</LogonType><RunLevel>HighestAvailable</RunLevel></Principal></Principals>" +
                    "<Settings><MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>" +
                    "<DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>" +
                    "<StopIfGoingOnBatteries>false</StopIfGoingOnBatteries><AllowHardTerminate>true</AllowHardTerminate>" +
                    "<StartWhenAvailable>false</StartWhenAvailable><RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>" +
                    "<AllowStartOnDemand>true</AllowStartOnDemand><Enabled>true</Enabled><Hidden>false</Hidden>" +
                    "<RunOnlyIfIdle>false</RunOnlyIfIdle><WakeToRun>false</WakeToRun>" +
                    "<ExecutionTimeLimit>PT0S</ExecutionTimeLimit><Priority>7</Priority></Settings>" +
                    "<Actions Context=\"Author\"><Exec><Command>" + command + "</Command>" +
                    "<WorkingDirectory>" + workingDirectory + "</WorkingDirectory></Exec></Actions></Task>";
                File.WriteAllText(xmlPath, xml, Encoding.Unicode);
                string output;
                return RunSchtasks("/Create /TN \"" + TaskName(key) + "\" /XML \"" + xmlPath + "\" /F", out output);
            }
            catch { return 11; }
            finally
            {
                try { File.Delete(xmlPath); } catch { }
            }
        }

        private static bool IsConfigured(string key, string path)
        {
            string output;
            if (RunSchtasks("/Query /TN \"" + TaskName(key) + "\" /XML", out output) != 0 ||
                String.IsNullOrWhiteSpace(output)) return false;
            try
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(output);
                XmlNodeList commands = document.GetElementsByTagName("Command");
                return commands.Count == 1 && String.Equals(Path.GetFullPath(commands[0].InnerText),
                    Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        private static bool IsAllowedTarget(string key, string path)
        {
            try
            {
                if (!String.Equals(key, ConspitKey, StringComparison.OrdinalIgnoreCase) ||
                    String.IsNullOrWhiteSpace(path) || !File.Exists(path) || path.IndexOf('"') >= 0 ||
                    !String.Equals(Path.GetFileName(path), "ConspitLink2.0.exe", StringComparison.OrdinalIgnoreCase))
                    return false;

                string fullPath = Path.GetFullPath(path);
                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                return IsUnder(fullPath, programFiles) || IsUnder(fullPath, programFilesX86);
            }
            catch { return false; }
        }

        private static bool IsUnder(string path, string root)
        {
            if (String.IsNullOrWhiteSpace(root)) return false;
            string normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
        }

        private static string TaskName(string key)
        {
            return String.Equals(key, ConspitKey, StringComparison.OrdinalIgnoreCase) ? ConspitTaskName : "";
        }

        private static int RunSchtasks(string arguments, out string output)
        {
            output = "";
            try
            {
                ProcessStartInfo info = new ProcessStartInfo("schtasks.exe", arguments);
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                Process process = Process.Start(info);
                if (process == null) return -1;
                string standardOutput = process.StandardOutput.ReadToEnd();
                string standardError = process.StandardError.ReadToEnd();
                process.WaitForExit(10000);
                int exitCode = process.HasExited ? process.ExitCode : -1;
                if (!process.HasExited) try { process.Kill(); } catch { }
                process.Dispose();
                output = String.IsNullOrWhiteSpace(standardError) ? standardOutput : standardError;
                return exitCode;
            }
            catch (Exception ex)
            {
                output = ex.Message;
                return -1;
            }
        }
    }

    public static class StartupManager
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "iRacing Digital Teammate";
        private const string LegacyValueName = "iRacing Teammate";

        private static string ShortcutPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                    "iRacing Digital Teammate.lnk");
            }
        }

        private static string LegacyShortcutPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                    "iRacing Teammate.lnk");
            }
        }

        public static bool IsEnabled()
        {
            try
            {
                return File.Exists(ShortcutPath) || File.Exists(LegacyShortcutPath);
            }
            catch { return false; }
        }

        public static bool SetEnabled(bool enabled, string executablePath)
        {
            try
            {
                RemoveLegacyRunValue();
                if (File.Exists(LegacyShortcutPath)) File.Delete(LegacyShortcutPath);
                if (!enabled)
                {
                    if (File.Exists(ShortcutPath)) File.Delete(ShortcutPath);
                    return true;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(ShortcutPath));
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return false;
                object shell = Activator.CreateInstance(shellType);
                object shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod,
                    null, shell, new object[] { ShortcutPath });
                Type shortcutType = shortcut.GetType();
                shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut,
                    new object[] { executablePath });
                shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut,
                    new object[] { Path.GetDirectoryName(executablePath) });
                shortcutType.InvokeMember("Arguments", BindingFlags.SetProperty, null, shortcut,
                    new object[] { "--minimized" });
                shortcutType.InvokeMember("Description", BindingFlags.SetProperty, null, shortcut,
                    new object[] { "Start iRacing Digital Teammate minimized with Windows" });
                shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
                return true;
            }
            catch { return false; }
        }

        private static void RemoveLegacyRunValue()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, true))
                    if (key != null)
                    {
                        key.DeleteValue(ValueName, false);
                        key.DeleteValue(LegacyValueName, false);
                    }
            }
            catch { }
        }
    }

    public class UpdateCheckResult
    {
        public bool Configured;
        public bool UpdateAvailable;
        public string LatestVersion;
        public string ReleaseUrl;
        public string InstallerUrl;
        public string ChecksumUrl;
        public string Error;
    }

    public class UpdateDownloadResult
    {
        public string InstallerPath;
        public string Error;

        public bool Succeeded
        {
            get { return !String.IsNullOrWhiteSpace(InstallerPath) && String.IsNullOrWhiteSpace(Error); }
        }
    }

    public static class UpdateChecker
    {
        public static string ResolveRepository(string configuredRepository)
        {
            if (!String.IsNullOrWhiteSpace(configuredRepository)) return configuredRepository.Trim();
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UpdateRepository"))
                using (StreamReader reader = stream == null ? null : new StreamReader(stream))
                    return reader == null ? "" : reader.ReadToEnd().Trim();
            }
            catch { return ""; }
        }

        public static UpdateCheckResult Check(string repository)
        {
            UpdateCheckResult result = new UpdateCheckResult();
            if (String.IsNullOrWhiteSpace(repository) || !repository.Contains("/"))
            {
                result.Configured = false;
                return result;
            }

            result.Configured = true;
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                string endpoint = "https://api.github.com/repos/" + repository.Trim().Trim('/') + "/releases/latest";
                string json;
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "DDS-iRacing-Digital-Teammate");
                    client.Headers.Add("Accept", "application/vnd.github+json");
                    json = client.DownloadString(endpoint);
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> release = serializer.Deserialize<Dictionary<string, object>>(json);
                result.LatestVersion = release.ContainsKey("tag_name") ? Convert.ToString(release["tag_name"]) : "";
                result.ReleaseUrl = release.ContainsKey("html_url") ? Convert.ToString(release["html_url"]) :
                    "https://github.com/" + repository + "/releases/latest";

                object assetsValue;
                IEnumerable assets = release.TryGetValue("assets", out assetsValue) ? assetsValue as IEnumerable : null;
                if (assets != null)
                {
                    foreach (object assetValue in assets)
                    {
                        Dictionary<string, object> asset = assetValue as Dictionary<string, object>;
                        if (asset == null) continue;
                        string name = asset.ContainsKey("name") ? Convert.ToString(asset["name"]) : "";
                        string url = asset.ContainsKey("browser_download_url") ? Convert.ToString(asset["browser_download_url"]) : "";
                        if (name.StartsWith("iRacing-Digital-Teammate-Setup-", StringComparison.OrdinalIgnoreCase) &&
                            name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                            result.InstallerUrl = url;
                        else if (name.StartsWith("iRacing-Digital-Teammate-Setup-", StringComparison.OrdinalIgnoreCase) &&
                            name.EndsWith(".exe.sha256", StringComparison.OrdinalIgnoreCase))
                            result.ChecksumUrl = url;
                    }
                }

                string normalized = (result.LatestVersion ?? "").Trim();
                if (normalized.StartsWith("v", StringComparison.OrdinalIgnoreCase)) normalized = normalized.Substring(1);
                Version latest;
                Version current = Assembly.GetExecutingAssembly().GetName().Version;
                result.UpdateAvailable = Version.TryParse(normalized, out latest) && latest > current;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }
            return result;
        }
    }

    public static class UpdateInstaller
    {
        public static UpdateDownloadResult DownloadAndVerify(UpdateCheckResult update)
        {
            UpdateDownloadResult result = new UpdateDownloadResult();
            try
            {
                Uri installerUri;
                Uri checksumUri;
                if (update == null || !Uri.TryCreate(update.InstallerUrl, UriKind.Absolute, out installerUri) ||
                    !Uri.TryCreate(update.ChecksumUrl, UriKind.Absolute, out checksumUri) ||
                    installerUri.Scheme != Uri.UriSchemeHttps || checksumUri.Scheme != Uri.UriSchemeHttps ||
                    !installerUri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) ||
                    !checksumUri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("This release does not contain a trusted installer and checksum pair.");

                string version = SafePathPart(update.LatestVersion);
                string directory = Path.Combine(Path.GetTempPath(), "iRacing Digital Teammate Update", version);
                Directory.CreateDirectory(directory);
                string installerPath = Path.Combine(directory, "iRacing-Digital-Teammate-Update.exe");
                string checksumPath = installerPath + ".sha256";

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "DDS-iRacing-Digital-Teammate");
                    client.DownloadFile(installerUri, installerPath);
                    client.DownloadFile(checksumUri, checksumPath);
                }

                string checksumText = File.ReadAllText(checksumPath).Trim();
                string expected = checksumText.Split(new char[] { ' ', '\t', '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                string actual;
                using (SHA256 sha = SHA256.Create())
                using (FileStream stream = File.OpenRead(installerPath))
                    actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();

                if (String.IsNullOrWhiteSpace(expected) || expected.Length != 64 ||
                    !actual.Equals(expected, StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Delete(installerPath); } catch { }
                    throw new InvalidDataException("The downloaded update failed SHA-256 verification.");
                }

                result.InstallerPath = installerPath;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }
            return result;
        }

        public static void Launch(string installerPath)
        {
            if (String.IsNullOrWhiteSpace(installerPath) || !File.Exists(installerPath))
                throw new FileNotFoundException("The verified update installer could not be found.", installerPath);

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = installerPath;
            start.Arguments = "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /CLOSEAPPLICATIONS /SP- /DDSUPDATE=1";
            start.UseShellExecute = false;
            Process.Start(start);
        }

        private static string SafePathPart(string value)
        {
            string safe = String.IsNullOrWhiteSpace(value) ? "latest" : value.Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars()) safe = safe.Replace(invalid, '_');
            return safe;
        }
    }
}
