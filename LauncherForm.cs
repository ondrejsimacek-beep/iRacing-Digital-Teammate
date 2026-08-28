using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace DigitalDownforceSimRacing.IRacingTeammate
{
    public static class Livery
    {
        public static readonly Color Background = Color.FromArgb(9, 0, 25);
        public static readonly Color Sidebar = Color.FromArgb(13, 2, 34);
        public static readonly Color Surface = Color.FromArgb(22, 8, 47);
        public static readonly Color SurfaceLight = Color.FromArgb(33, 15, 66);
        public static readonly Color Carbon = Color.FromArgb(12, 1, 29);
        public static readonly Color Gold = Color.FromArgb(224, 45, 238);
        public static readonly Color GoldBright = Color.FromArgb(244, 94, 255);
        public static readonly Color Silver = Color.FromArgb(218, 231, 255);
        public static readonly Color Blue = Color.FromArgb(0, 222, 246);
        public static readonly Color Text = Color.FromArgb(247, 248, 255);
        public static readonly Color Muted = Color.FromArgb(154, 165, 195);
        public static readonly Color Border = Color.FromArgb(58, 35, 99);
        public static readonly Color Success = Color.FromArgb(46, 212, 184);
        public static readonly Color Error = Color.FromArgb(255, 92, 122);
    }

    public class LauncherForm : Form
    {
        private readonly List<AppDefinition> definitions;
        private readonly SettingsStore store;
        private readonly LauncherSettings settings;
        private readonly ProcessController processes;
        private readonly Dictionary<string, AppCard> cards;
        private readonly FlowLayoutPanel cardsFlow;
        private readonly Label activityLabel;
        private readonly Label stackStatusLabel;
        private readonly LiveryButton startButton;
        private readonly LiveryButton stopButton;
        private readonly LiveryButton hiddenButton;
        private LiveryButton startupButton;
        private LiveryButton updateButton;
        private LiveryButton autoModeButton;
        private readonly System.Windows.Forms.Timer refreshTimer;
        private volatile bool operationRunning;
        private volatile bool autoTransitionRunning;
        private bool showHidden;
        private bool sessionWasRunning;
        private readonly bool previewMode;
        private readonly bool startMinimized;
        private NotifyIcon trayIcon;
        private bool exitRequested;
        private bool trayHintShown;

        public LauncherForm(bool previewMode, bool startMinimized)
        {
            this.previewMode = previewMode;
            this.startMinimized = startMinimized;
            definitions = AppCatalog.Create();
            store = new SettingsStore();
            settings = store.Load(definitions);
            processes = new ProcessController();
            cards = new Dictionary<string, AppCard>();

            Text = "iRacing Teammate — Digital Downforce Sim Racing";
            Size = new Size(1280, 820);
            MinimumSize = new Size(1080, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Livery.Background;
            ForeColor = Livery.Text;
            Font = new Font("Segoe UI", 9F);
            Icon = CreateAppIcon();
            if (startMinimized)
            {
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
                Opacity = 0D;
            }

            TableLayoutPanel shell = new TableLayoutPanel();
            shell.Dock = DockStyle.Fill;
            shell.ColumnCount = 2;
            shell.RowCount = 1;
            shell.BackColor = Livery.Background;
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 236F));
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            Controls.Add(shell);

            shell.Controls.Add(BuildSidebar(), 0, 0);

            TableLayoutPanel main = new TableLayoutPanel();
            main.Dock = DockStyle.Fill;
            main.Padding = new Padding(28, 24, 28, 20);
            main.BackColor = Livery.Background;
            main.RowCount = 4;
            main.ColumnCount = 1;
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 166F));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            shell.Controls.Add(main, 1, 0);

            HeroPanel hero = new HeroPanel();
            hero.Dock = DockStyle.Fill;
            hero.Margin = new Padding(0, 0, 0, 14);
            main.Controls.Add(hero, 0, 0);

            Panel actions = new Panel();
            actions.Dock = DockStyle.Fill;
            actions.BackColor = Livery.Background;
            main.Controls.Add(actions, 0, 1);

            stackStatusLabel = new Label();
            stackStatusLabel.Text = "RACE STACK";
            stackStatusLabel.ForeColor = Livery.Muted;
            stackStatusLabel.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            stackStatusLabel.AutoSize = true;
            stackStatusLabel.Location = new Point(0, 4);
            actions.Controls.Add(stackStatusLabel);

            Label sectionTitle = new Label();
            sectionTitle.Text = "Your software lineup";
            sectionTitle.ForeColor = Livery.Text;
            sectionTitle.Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold);
            sectionTitle.AutoSize = true;
            sectionTitle.Location = new Point(-1, 24);
            actions.Controls.Add(sectionTitle);

            FlowLayoutPanel actionButtons = new FlowLayoutPanel();
            actionButtons.Dock = DockStyle.Right;
            actionButtons.Width = 555;
            actionButtons.FlowDirection = FlowDirection.RightToLeft;
            actionButtons.WrapContents = false;
            actionButtons.Padding = new Padding(0, 11, 0, 10);
            actionButtons.BackColor = Livery.Background;
            actions.Controls.Add(actionButtons);

            startButton = new LiveryButton("START RACE STACK", Livery.Gold, Color.FromArgb(18, 18, 18), 176);
            startButton.Click += delegate { StartStack(); };
            actionButtons.Controls.Add(startButton);

            stopButton = new LiveryButton("STOP LAUNCHED", Livery.SurfaceLight, Livery.Text, 145);
            stopButton.Click += delegate { StopStack(); };
            actionButtons.Controls.Add(stopButton);

            LiveryButton scanButton = new LiveryButton("RESCAN", Livery.SurfaceLight, Livery.Silver, 96);
            scanButton.Click += delegate { Rescan(); };
            actionButtons.Controls.Add(scanButton);

            hiddenButton = new LiveryButton("HIDDEN", Livery.SurfaceLight, Livery.Silver, 104);
            hiddenButton.Click += delegate
            {
                showHidden = !showHidden;
                ApplyHiddenFilter();
            };
            actionButtons.Controls.Add(hiddenButton);

            cardsFlow = new FlowLayoutPanel();
            cardsFlow.Dock = DockStyle.Fill;
            cardsFlow.AutoScroll = true;
            cardsFlow.WrapContents = true;
            cardsFlow.BackColor = Livery.Background;
            cardsFlow.Padding = new Padding(0, 2, 0, 8);
            cardsFlow.Margin = new Padding(0);
            main.Controls.Add(cardsFlow, 0, 2);
            cardsFlow.Resize += delegate { ResizeCards(); };

            foreach (AppDefinition definition in definitions)
            {
                AppSetting setting = FindSetting(definition.Key);
                AppCard card = new AppCard(definition, setting, previewMode);
                card.ToggleChanged += delegate { store.Save(settings); UpdateStackStatus(); };
                card.BrowseRequested += delegate(object sender, EventArgs e) { BrowseFor((AppCard)sender); };
                card.HideRequested += delegate(object sender, EventArgs e)
                {
                    AppCard changed = (AppCard)sender;
                    changed.Setting.Hidden = !changed.Setting.Hidden;
                    store.Save(settings);
                    ApplyHiddenFilter();
                };
                cards[definition.Key] = card;
                cardsFlow.Controls.Add(card);
            }

            Panel activity = new Panel();
            activity.Dock = DockStyle.Fill;
            activity.BackColor = Livery.Sidebar;
            activity.Padding = new Padding(14, 9, 14, 0);
            main.Controls.Add(activity, 0, 3);

            Label activityDot = new Label();
            activityDot.Text = "●";
            activityDot.ForeColor = Livery.Gold;
            activityDot.AutoSize = true;
            activityDot.Location = new Point(13, 11);
            activity.Controls.Add(activityDot);

            activityLabel = new Label();
            activityLabel.Text = "Ready. Choose your lineup and start the race stack.";
            activityLabel.ForeColor = Livery.Muted;
            activityLabel.AutoEllipsis = true;
            activityLabel.Location = new Point(34, 11);
            activityLabel.Size = new Size(770, 20);
            activityLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            activity.Controls.Add(activityLabel);

            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 1200;
            refreshTimer.Tick += delegate { RefreshStatuses(); };
            refreshTimer.Start();

            if (!previewMode) InitializeTray();
            Shown += delegate
            {
                ApplyHiddenFilter();
                ResizeCards();
                RefreshStatuses();
                UpdateStackStatus();
                if (this.startMinimized)
                {
                    Opacity = 1D;
                    BeginInvoke(new MethodInvoker(delegate { MinimizeToTray(false); }));
                }
            };
        }

        private Control BuildSidebar()
        {
            Panel sidebar = new Panel();
            sidebar.Dock = DockStyle.Fill;
            sidebar.BackColor = Livery.Sidebar;
            sidebar.Padding = new Padding(22, 26, 22, 22);

            BrandLogo logo = new BrandLogo();
            logo.Location = new Point(20, 26);
            logo.Size = new Size(70, 58);
            sidebar.Controls.Add(logo);

            Label brand = new Label();
            brand.Text = "DIGITAL DOWNFORCE\nSIM RACING";
            brand.ForeColor = Livery.Text;
            brand.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            brand.Location = new Point(94, 34);
            brand.Size = new Size(132, 45);
            sidebar.Controls.Add(brand);

            Panel goldRule = new Panel();
            goldRule.BackColor = Livery.Gold;
            goldRule.Location = new Point(22, 108);
            goldRule.Size = new Size(42, 3);
            sidebar.Controls.Add(goldRule);

            AddNav(sidebar, "01", "RACE STACK", 145, true);
            AddNav(sidebar, "02", "APPLICATIONS", 194, false);
            AddNav(sidebar, "03", "LAUNCH ORDER", 243, false);

            Label label = new Label();
            label.Text = "PIT WALL";
            label.ForeColor = Livery.Muted;
            label.Font = new Font("Segoe UI Semibold", 7.5F, FontStyle.Bold);
            label.Location = new Point(23, 318);
            label.AutoSize = true;
            sidebar.Controls.Add(label);

            Panel info = new Panel();
            info.BackColor = Livery.Carbon;
            info.Location = new Point(22, 344);
            info.Size = new Size(192, 116);
            info.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            sidebar.Controls.Add(info);

            Label infoTitle = new Label();
            infoTitle.Text = "SESSION CONTROL";
            infoTitle.ForeColor = Livery.Silver;
            infoTitle.Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold);
            infoTitle.Location = new Point(14, 14);
            infoTitle.AutoSize = true;
            info.Controls.Add(infoTitle);

            Label infoText = new Label();
            infoText.Text = "Auto Mode follows each iRacing session. Only processes launched here are stopped.";
            infoText.ForeColor = Livery.Muted;
            infoText.Font = new Font("Segoe UI", 8F);
            infoText.Location = new Point(14, 39);
            infoText.Size = new Size(162, 62);
            info.Controls.Add(infoText);

            autoModeButton = new LiveryButton("AUTO MODE", Livery.SurfaceLight, Livery.Silver, 192);
            autoModeButton.Location = new Point(22, 498);
            autoModeButton.Click += delegate { ToggleAutoMode(); };
            sidebar.Controls.Add(autoModeButton);

            startupButton = new LiveryButton("START WITH WINDOWS", Livery.SurfaceLight, Livery.Silver, 192);
            startupButton.Location = new Point(22, 548);
            startupButton.Click += delegate { ToggleStartup(); };
            sidebar.Controls.Add(startupButton);

            updateButton = new LiveryButton("CHECK FOR UPDATES", Livery.SurfaceLight, Livery.Silver, 192);
            updateButton.Location = new Point(22, 598);
            updateButton.Click += delegate { CheckForUpdates(); };
            sidebar.Controls.Add(updateButton);

            LiveryButton folder = new LiveryButton("OPEN CONFIG", Livery.SurfaceLight, Livery.Silver, 192);
            folder.Location = new Point(22, 478);
            folder.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            folder.Click += delegate
            {
                Directory.CreateDirectory(store.DirectoryPath);
                System.Diagnostics.Process.Start("explorer.exe", store.DirectoryPath);
            };
            sidebar.Controls.Add(folder);

            Label footer = new Label();
            footer.Text = "iRACING TEAMMATE  •  " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) +
                "\nBY DDS";
            footer.ForeColor = Color.FromArgb(91, 98, 106);
            footer.Font = new Font("Segoe UI Semibold", 7.3F, FontStyle.Bold);
            footer.Location = new Point(22, 735);
            footer.Size = new Size(190, 38);
            footer.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            sidebar.Controls.Add(footer);

            sidebar.Resize += delegate
            {
                autoModeButton.Top = Math.Max(450, sidebar.ClientSize.Height - 262);
                startupButton.Top = Math.Max(500, sidebar.ClientSize.Height - 212);
                updateButton.Top = Math.Max(550, sidebar.ClientSize.Height - 162);
                folder.Top = Math.Max(500, sidebar.ClientSize.Height - 112);
                footer.Top = Math.Max(550, sidebar.ClientSize.Height - 55);
            };

            RefreshAutoModeButton();
            if (!previewMode && (settings.AutoModeEnabled || StartupManager.IsEnabled()))
                StartupManager.SetEnabled(true, Application.ExecutablePath);
            RefreshStartupButton();

            return sidebar;
        }

        private static void AddNav(Control parent, string number, string text, int top, bool selected)
        {
            Panel row = new Panel();
            row.Location = new Point(0, top);
            row.Size = new Size(236, 40);
            row.BackColor = selected ? Color.FromArgb(26, 28, 31) : Livery.Sidebar;
            parent.Controls.Add(row);

            if (selected)
            {
                Panel rail = new Panel();
                rail.BackColor = Livery.Gold;
                rail.Location = new Point(0, 0);
                rail.Size = new Size(3, 40);
                row.Controls.Add(rail);
            }
            Label numberLabel = new Label();
            numberLabel.Text = number;
            numberLabel.ForeColor = selected ? Livery.Gold : Color.FromArgb(77, 83, 91);
            numberLabel.Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold);
            numberLabel.Location = new Point(22, 12);
            numberLabel.AutoSize = true;
            row.Controls.Add(numberLabel);

            Label textLabel = new Label();
            textLabel.Text = text;
            textLabel.ForeColor = selected ? Livery.Text : Livery.Muted;
            textLabel.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            textLabel.Location = new Point(57, 11);
            textLabel.AutoSize = true;
            row.Controls.Add(textLabel);
        }

        private AppSetting FindSetting(string key)
        {
            return settings.Apps.First(delegate(AppSetting item) { return item.Key == key; });
        }

        private void ToggleStartup()
        {
            bool enable = !StartupManager.IsEnabled();
            string executable = Application.ExecutablePath;
            if (StartupManager.SetEnabled(enable, executable))
            {
                RefreshStartupButton();
                SetActivity(enable ? "Teammate will start when you sign in to Windows." :
                    "Windows startup disabled.", enable ? Livery.Success : Livery.Muted);
            }
            else
            {
                MessageBox.Show(this, "Windows startup could not be changed.", "iRacing Teammate",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ToggleAutoMode()
        {
            settings.AutoModeEnabled = !settings.AutoModeEnabled;
            store.Save(settings);
            if (settings.AutoModeEnabled)
            {
                StartupManager.SetEnabled(true, Application.ExecutablePath);
                sessionWasRunning = false;
                SetActivity("Auto Mode enabled — waiting for an iRacing session.", Livery.Success);
            }
            else
            {
                SetActivity("Auto Mode disabled. Running applications were left untouched.", Livery.Muted);
            }
            RefreshAutoModeButton();
            RefreshStartupButton();
        }

        private void RefreshAutoModeButton()
        {
            if (autoModeButton == null) return;
            autoModeButton.Text = settings.AutoModeEnabled ? "AUTO MODE  •  ON" : "AUTO MODE  •  OFF";
            autoModeButton.ForeColor = settings.AutoModeEnabled ? Livery.GoldBright : Livery.Silver;
        }

        private void RefreshStartupButton()
        {
            if (startupButton == null) return;
            bool enabled = StartupManager.IsEnabled();
            startupButton.Text = enabled ? "START WITH WINDOWS  •  ON" : "START WITH WINDOWS  •  OFF";
            startupButton.ForeColor = enabled ? Livery.GoldBright : Livery.Silver;
        }

        private void CheckForUpdates()
        {
            if (updateButton == null || !updateButton.Enabled) return;
            updateButton.Enabled = false;
            updateButton.Text = "CHECKING…";
            SetActivity("Checking GitHub Releases for updates…", Livery.GoldBright);

            Thread worker = new Thread(delegate()
            {
                string repository = UpdateChecker.ResolveRepository(settings.UpdateRepository);
                UpdateCheckResult result = UpdateChecker.Check(repository);
                Ui(delegate
                {
                    updateButton.Enabled = true;
                    updateButton.Text = "CHECK FOR UPDATES";

                    if (!result.Configured)
                    {
                        SetActivity("Update checker is ready; GitHub repository is not configured yet.", Livery.Muted);
                        DialogResult open = MessageBox.Show(this,
                            "The update checker is implemented, but the GitHub repository has not been assigned yet.\n\n" +
                            "After the repository is created, set UpdateRepository in settings.xml to owner/repository.\n\n" +
                            "Open the configuration folder now?",
                            "GitHub update source", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (open == DialogResult.Yes)
                        {
                            Directory.CreateDirectory(store.DirectoryPath);
                            System.Diagnostics.Process.Start("explorer.exe", store.DirectoryPath);
                        }
                        return;
                    }

                    if (!String.IsNullOrWhiteSpace(result.Error))
                    {
                        SetActivity("Update check failed.", Livery.Error);
                        MessageBox.Show(this, "GitHub update check failed:\n\n" + result.Error,
                            "iRacing Teammate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.UpdateAvailable)
                    {
                        SetActivity("A new version " + result.LatestVersion + " is available.", Livery.Success);
                        DialogResult download = MessageBox.Show(this,
                            "A new iRacing Teammate version " + result.LatestVersion + " is available.\n\n" +
                            "Open the GitHub release page?",
                            "Update available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (download == DialogResult.Yes && !String.IsNullOrWhiteSpace(result.ReleaseUrl))
                            System.Diagnostics.Process.Start(result.ReleaseUrl);
                    }
                    else
                    {
                        Version current = Assembly.GetExecutingAssembly().GetName().Version;
                        SetActivity("iRacing Teammate is up to date.", Livery.Success);
                        MessageBox.Show(this,
                            "You are running the latest version (" + current.ToString(3) + ").",
                            "No updates available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                });
            });
            worker.IsBackground = true;
            worker.Start();
        }

        private void ResizeCards()
        {
            int scrollbar = cardsFlow.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0;
            int usable = cardsFlow.ClientSize.Width - cardsFlow.Padding.Horizontal - scrollbar - 16;
            int columns = usable >= 700 ? 2 : 1;
            int width = Math.Max(360, usable / columns - 8);
            foreach (AppCard card in cards.Values) card.Width = width;
        }

        private void ApplyHiddenFilter()
        {
            int hiddenCount = settings.Apps.Count(delegate(AppSetting item) { return item.Hidden; });
            foreach (KeyValuePair<string, AppCard> pair in cards)
                pair.Value.Visible = showHidden || !pair.Value.Setting.Hidden;
            hiddenButton.Text = showHidden ? "HIDE HIDDEN" : "HIDDEN (" + hiddenCount + ")";
            hiddenButton.ForeColor = showHidden ? Livery.GoldBright : Livery.Silver;
            ResizeCards();
        }

        private void BrowseFor(AppCard card)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select " + card.Definition.Name + " executable";
            dialog.Filter = "Applications (*.exe)|*.exe|All files (*.*)|*.*";
            dialog.CheckFileExists = true;
            if (!String.IsNullOrWhiteSpace(card.Setting.Path) && File.Exists(card.Setting.Path))
                dialog.InitialDirectory = Path.GetDirectoryName(card.Setting.Path);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                card.Setting.Path = dialog.FileName;
                card.Setting.Enabled = true;
                card.RefreshView(processes.IsRunning(card.Definition));
                store.Save(settings);
                SetActivity(card.Definition.Name + " is configured.", Livery.Success);
                UpdateStackStatus();
            }
            dialog.Dispose();
        }

        private void Rescan()
        {
            int found = 0;
            foreach (AppDefinition definition in definitions)
            {
                AppSetting setting = FindSetting(definition.Key);
                if (String.IsNullOrWhiteSpace(setting.Path) || !File.Exists(setting.Path))
                {
                    string detected = AppCatalog.DetectPath(definition);
                    if (!String.IsNullOrWhiteSpace(detected))
                    {
                        setting.Path = detected;
                        found++;
                    }
                }
                cards[definition.Key].RefreshView(processes.IsRunning(definition));
            }
            store.Save(settings);
            SetActivity(found == 0 ? "Scan complete. No new applications found." :
                "Scan complete. Found " + found + " new application" + (found == 1 ? "." : "s."),
                found == 0 ? Livery.Muted : Livery.Success);
            UpdateStackStatus();
        }

        private void StartStack()
        {
            if (operationRunning) return;
            operationRunning = true;
            SetButtonsEnabled(false);
            Thread worker = new Thread(delegate()
            {
                List<AppDefinition> selected = definitions.Where(delegate(AppDefinition definition)
                {
                    return FindSetting(definition.Key).Enabled;
                }).ToList();

                int launched = 0;
                foreach (AppDefinition definition in selected)
                {
                    AppSetting setting = FindSetting(definition.Key);
                    if (String.IsNullOrWhiteSpace(setting.Path) || !File.Exists(setting.Path))
                    {
                        Ui(delegate { SetActivity("Skipped " + definition.Name + " — select its executable first.", Livery.Error); });
                        continue;
                    }
                    Ui(delegate { SetActivity("Starting " + definition.Name + "…", Livery.GoldBright); });
                    string error;
                    if (processes.Launch(definition, setting.Path, out error))
                    {
                        launched++;
                        Ui(delegate { cards[definition.Key].RefreshView(true); });
                        if (setting.DelaySeconds > 0) Thread.Sleep(setting.DelaySeconds * 1000);
                    }
                    else
                    {
                        Ui(delegate { SetActivity("Could not start " + definition.Name + ": " + error, Livery.Error); });
                    }
                }
                Ui(delegate
                {
                    SetActivity("Race stack ready — " + launched + " application" + (launched == 1 ? "" : "s") + " started.", Livery.Success);
                    operationRunning = false;
                    SetButtonsEnabled(true);
                    RefreshStatuses();
                });
            });
            worker.IsBackground = true;
            worker.Start();
        }

        private void StopStack()
        {
            if (operationRunning) return;
            operationRunning = true;
            SetButtonsEnabled(false);
            Thread worker = new Thread(delegate()
            {
                int stopped = 0;
                for (int i = definitions.Count - 1; i >= 0; i--)
                {
                    AppDefinition definition = definitions[i];
                    if (processes.StopTracked(definition)) stopped++;
                }
                Ui(delegate
                {
                    SetActivity(stopped == 0 ? "No Teammate-launched processes are running." :
                        "Stopped " + stopped + " launched application" + (stopped == 1 ? "." : "s."),
                        stopped == 0 ? Livery.Muted : Livery.GoldBright);
                    operationRunning = false;
                    SetButtonsEnabled(true);
                    RefreshStatuses();
                });
            });
            worker.IsBackground = true;
            worker.Start();
        }

        private void RefreshStatuses()
        {
            foreach (AppDefinition definition in definitions)
                cards[definition.Key].RefreshView(processes.IsRunning(definition));
            UpdateStackStatus();
            HandleAutoMode();
        }

        private void HandleAutoMode()
        {
            if (previewMode) return;
            bool sessionRunning = ProcessController.IsIRacingSessionRunning();
            if (!settings.AutoModeEnabled)
            {
                sessionWasRunning = sessionRunning;
                return;
            }

            if (sessionRunning && !sessionWasRunning)
            {
                sessionWasRunning = true;
                StartAutoStack();
            }
            else if (!sessionRunning && sessionWasRunning)
            {
                sessionWasRunning = false;
                ScheduleAutoStop();
            }
        }

        private void StartAutoStack()
        {
            if (operationRunning) return;
            operationRunning = true;
            SetButtonsEnabled(false);
            SetActivity("iRacing session detected — starting companion applications…", Livery.GoldBright);

            Thread worker = new Thread(delegate()
            {
                int launched = 0;
                List<AppDefinition> selected = definitions.Where(delegate(AppDefinition definition)
                {
                    return definition.Key != "iracing" && FindSetting(definition.Key).Enabled;
                }).ToList();

                foreach (AppDefinition definition in selected)
                {
                    if (!ProcessController.IsIRacingSessionRunning()) break;
                    AppSetting setting = FindSetting(definition.Key);
                    if (String.IsNullOrWhiteSpace(setting.Path) || !File.Exists(setting.Path))
                    {
                        Ui(delegate { SetActivity("Auto Mode skipped " + definition.Name + " — executable not configured.", Livery.Error); });
                        continue;
                    }

                    bool wasRunning = processes.IsRunning(definition);
                    string error;
                    if (processes.Launch(definition, setting.Path, out error))
                    {
                        if (!wasRunning) launched++;
                        Ui(delegate { cards[definition.Key].RefreshView(true); });
                        if (!wasRunning && setting.DelaySeconds > 0) Thread.Sleep(setting.DelaySeconds * 1000);
                    }
                    else
                    {
                        Ui(delegate { SetActivity("Auto Mode could not start " + definition.Name + ": " + error, Livery.Error); });
                    }
                }

                Ui(delegate
                {
                    SetActivity("Auto Mode active — " + launched + " companion application" +
                        (launched == 1 ? "" : "s") + " started for this session.", Livery.Success);
                    operationRunning = false;
                    SetButtonsEnabled(true);
                    RefreshStatuses();
                });
            });
            worker.IsBackground = true;
            worker.Start();
        }

        private void ScheduleAutoStop()
        {
            if (autoTransitionRunning) return;
            autoTransitionRunning = true;
            SetActivity("iRacing session ended — confirming shutdown…", Livery.GoldBright);

            Thread worker = new Thread(delegate()
            {
                Thread.Sleep(3000);
                if (ProcessController.IsIRacingSessionRunning())
                {
                    autoTransitionRunning = false;
                    return;
                }

                while (operationRunning)
                {
                    Thread.Sleep(200);
                    if (ProcessController.IsIRacingSessionRunning())
                    {
                        autoTransitionRunning = false;
                        return;
                    }
                }

                operationRunning = true;
                int stopped = 0;
                for (int i = definitions.Count - 1; i >= 0; i--)
                {
                    AppDefinition definition = definitions[i];
                    if (definition.Key != "iracing" && processes.StopTracked(definition)) stopped++;
                }

                Ui(delegate
                {
                    SetActivity("Session cleanup complete — " + stopped + " companion application" +
                        (stopped == 1 ? "" : "s") + " stopped.", Livery.Success);
                    operationRunning = false;
                    autoTransitionRunning = false;
                    SetButtonsEnabled(true);
                    RefreshStatuses();
                });
            });
            worker.IsBackground = true;
            worker.Start();
        }

        private void UpdateStackStatus()
        {
            int selected = settings.Apps.Count(delegate(AppSetting item) { return item.Enabled; });
            int configured = settings.Apps.Count(delegate(AppSetting item) { return item.Enabled && File.Exists(item.Path); });
            stackStatusLabel.Text = "RACE STACK  •  " + selected + " SELECTED  •  " + configured + " READY";
        }

        private void SetButtonsEnabled(bool enabled)
        {
            startButton.Enabled = enabled;
            stopButton.Enabled = enabled;
        }

        private void SetActivity(string text, Color color)
        {
            activityLabel.Text = text;
            activityLabel.ForeColor = color;
        }

        private void Ui(MethodInvoker action)
        {
            if (IsDisposed) return;
            try { BeginInvoke(action); } catch { }
        }

        private void InitializeTray()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Livery.Surface;
            menu.ForeColor = Livery.Text;

            ToolStripMenuItem openItem = new ToolStripMenuItem("Open iRacing Teammate");
            openItem.Font = new Font(openItem.Font, FontStyle.Bold);
            openItem.Click += delegate { RestoreFromTray(); };
            menu.Items.Add(openItem);
            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += delegate { ExitFromTray(); };
            menu.Items.Add(exitItem);

            trayIcon = new NotifyIcon();
            trayIcon.Icon = Icon;
            trayIcon.Text = "iRacing Teammate — Digital Downforce Sim Racing";
            trayIcon.ContextMenuStrip = menu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += delegate { RestoreFromTray(); };

            Resize += delegate
            {
                if (WindowState == FormWindowState.Minimized)
                    BeginInvoke(new MethodInvoker(delegate { MinimizeToTray(false); }));
            };
            FormClosing += delegate(object sender, FormClosingEventArgs e)
            {
                if (!exitRequested && e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    MinimizeToTray(true);
                }
            };
            FormClosed += delegate
            {
                if (trayIcon != null)
                {
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                    trayIcon = null;
                }
                menu.Dispose();
            };
        }

        private void MinimizeToTray(bool showHint)
        {
            Hide();
            ShowInTaskbar = false;
            if (showHint && !trayHintShown && trayIcon != null)
            {
                trayIcon.ShowBalloonTip(2500, "iRacing Teammate",
                    "Teammate is still running and waiting for an iRacing session.", ToolTipIcon.Info);
                trayHintShown = true;
            }
        }

        private void RestoreFromTray()
        {
            ShowInTaskbar = true;
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
            BringToFront();
        }

        private void ExitFromTray()
        {
            exitRequested = true;
            if (trayIcon != null) trayIcon.Visible = false;
            refreshTimer.Stop();
            Close();
        }

        private static Icon CreateAppIcon()
        {
            Bitmap bitmap = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.Clear(Livery.Background);
                BrandAsset.DrawIcon(g, new Rectangle(1, 1, 30, 30));
            }
            return Icon.FromHandle(bitmap.GetHicon());
        }
    }

    public class AppCard : Panel
    {
        public readonly AppDefinition Definition;
        public readonly AppSetting Setting;
        private readonly CheckBox enabledCheck;
        private readonly Label pathLabel;
        private readonly Label stateLabel;
        private readonly Label dotLabel;
        private readonly Panel accent;
        private readonly LiveryButton hideButton;
        private readonly bool previewMode;

        public event EventHandler ToggleChanged;
        public event EventHandler BrowseRequested;
        public event EventHandler HideRequested;

        public AppCard(AppDefinition definition, AppSetting setting, bool isPreview)
        {
            Definition = definition;
            Setting = setting;
            previewMode = isPreview;
            Height = 112;
            Width = 450;
            BackColor = Livery.Surface;
            Margin = new Padding(0, 0, 14, 14);
            Padding = new Padding(16);

            accent = new Panel();
            accent.BackColor = Livery.Border;
            accent.Dock = DockStyle.Left;
            accent.Width = 3;
            Controls.Add(accent);

            Panel badge = new Panel();
            badge.Location = new Point(18, 18);
            badge.Size = new Size(52, 52);
            badge.BackColor = Livery.Carbon;
            Controls.Add(badge);

            Label initials = new Label();
            initials.Text = definition.Initials;
            initials.ForeColor = Livery.GoldBright;
            initials.Font = new Font("Segoe UI Semibold", definition.Initials.Length > 2 ? 9F : 12F, FontStyle.Bold);
            initials.TextAlign = ContentAlignment.MiddleCenter;
            initials.Dock = DockStyle.Fill;
            badge.Controls.Add(initials);

            Label name = new Label();
            name.Text = definition.Name;
            name.ForeColor = Livery.Text;
            name.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            name.Location = new Point(84, 16);
            name.Size = new Size(230, 24);
            name.AutoEllipsis = true;
            name.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            Controls.Add(name);

            Label category = new Label();
            category.Text = definition.Category;
            category.ForeColor = Livery.Blue;
            category.Font = new Font("Segoe UI Semibold", 7F, FontStyle.Bold);
            category.Location = new Point(85, 42);
            category.AutoSize = true;
            Controls.Add(category);

            pathLabel = new Label();
            pathLabel.ForeColor = Livery.Muted;
            pathLabel.Font = new Font("Segoe UI", 7.7F);
            pathLabel.Location = new Point(84, 70);
            pathLabel.Size = new Size(190, 19);
            pathLabel.AutoEllipsis = true;
            pathLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(pathLabel);

            enabledCheck = new CheckBox();
            enabledCheck.Text = "USE";
            enabledCheck.Checked = setting.Enabled;
            enabledCheck.ForeColor = Livery.Silver;
            enabledCheck.BackColor = Livery.Surface;
            enabledCheck.FlatStyle = FlatStyle.Flat;
            enabledCheck.Font = new Font("Segoe UI Semibold", 7.5F, FontStyle.Bold);
            enabledCheck.Size = new Size(55, 22);
            enabledCheck.Location = new Point(Width - 71, 15);
            enabledCheck.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            enabledCheck.CheckedChanged += delegate
            {
                Setting.Enabled = enabledCheck.Checked;
                accent.BackColor = Setting.Enabled ? Livery.Gold : Livery.Border;
                if (ToggleChanged != null) ToggleChanged(this, EventArgs.Empty);
            };
            Controls.Add(enabledCheck);

            LiveryButton browse = new LiveryButton("BROWSE", Livery.SurfaceLight, Livery.Silver, 76);
            browse.Height = 28;
            browse.Location = new Point(Width - 92, 67);
            browse.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            browse.Click += delegate { if (BrowseRequested != null) BrowseRequested(this, EventArgs.Empty); };
            Controls.Add(browse);

            hideButton = new LiveryButton("HIDE", Livery.SurfaceLight, Livery.Muted, 58);
            hideButton.Height = 28;
            hideButton.Location = new Point(Width - 158, 67);
            hideButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            hideButton.Click += delegate { if (HideRequested != null) HideRequested(this, EventArgs.Empty); };
            Controls.Add(hideButton);

            dotLabel = new Label();
            dotLabel.Text = "●";
            dotLabel.Font = new Font("Segoe UI", 8F);
            dotLabel.AutoSize = true;
            dotLabel.Location = new Point(18, 82);
            Controls.Add(dotLabel);

            stateLabel = new Label();
            stateLabel.Font = new Font("Segoe UI Semibold", 7F, FontStyle.Bold);
            stateLabel.Location = new Point(34, 82);
            stateLabel.Size = new Size(46, 17);
            Controls.Add(stateLabel);

            RefreshView(false);
        }

        public void RefreshView(bool running)
        {
            bool exists = previewMode || (!String.IsNullOrWhiteSpace(Setting.Path) && File.Exists(Setting.Path));
            string safePreviewPath = @"C:\Apps\" + Definition.Name.Replace(" ", "") + @"\" + Definition.ProcessName + ".exe";
            pathLabel.Text = exists ? (previewMode ? safePreviewPath : Setting.Path) : "Executable not selected";
            pathLabel.ForeColor = exists ? Livery.Muted : Livery.Error;
            if (running)
            {
                stateLabel.Text = "LIVE";
                stateLabel.ForeColor = Livery.Success;
                dotLabel.ForeColor = Livery.Success;
            }
            else if (exists)
            {
                stateLabel.Text = "READY";
                stateLabel.ForeColor = Livery.Muted;
                dotLabel.ForeColor = Livery.Muted;
            }
            else
            {
                stateLabel.Text = "SETUP";
                stateLabel.ForeColor = Livery.Error;
                dotLabel.ForeColor = Livery.Error;
            }
            if (enabledCheck.Checked != Setting.Enabled) enabledCheck.Checked = Setting.Enabled;
            accent.BackColor = Setting.Enabled ? Livery.Gold : Livery.Border;
            hideButton.Text = Setting.Hidden ? "SHOW" : "HIDE";
            hideButton.ForeColor = Setting.Hidden ? Livery.GoldBright : Livery.Muted;
        }
    }

    public class LiveryButton : Button
    {
        private readonly Color normalColor;
        private readonly Color textColor;

        public LiveryButton(string text, Color background, Color foreground, int width)
        {
            Text = text;
            normalColor = background;
            textColor = foreground;
            Width = width;
            Height = 42;
            Margin = new Padding(8, 0, 0, 0);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = background;
            ForeColor = foreground;
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold);
            MouseEnter += delegate { BackColor = ControlPaint.Light(normalColor, 0.08F); };
            MouseLeave += delegate { BackColor = normalColor; };
            EnabledChanged += delegate { ForeColor = Enabled ? textColor : Livery.Muted; };
        }
    }

    public class HeroPanel : Panel
    {
        public HeroPanel()
        {
            DoubleBuffered = true;
            BackColor = Livery.Carbon;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = Width;
            int h = Height;

            BrandAsset.DrawBanner(g, new Rectangle(Math.Max(0, w - 470), 0, 470, h));

            using (Brush subtle = new SolidBrush(Color.FromArgb(28, 10, 55)))
                for (int x = -h; x < Math.Max(600, w - 380); x += 24)
                    g.FillPolygon(subtle, new Point[] { new Point(x, h), new Point(x + 9, h), new Point(x + h, 0), new Point(x + h - 9, 0) });

            using (Brush dark = new SolidBrush(Color.FromArgb(236, Livery.Carbon)))
                g.FillRectangle(dark, 0, 0, Math.Max(560, w - 405), h);
            using (Brush magenta = new SolidBrush(Color.FromArgb(190, Livery.Gold)))
                g.FillPolygon(magenta, new Point[] { new Point(w - 420, h), new Point(w - 372, 0), new Point(w - 354, 0), new Point(w - 401, h) });
            using (Brush cyan = new SolidBrush(Color.FromArgb(190, Livery.Blue)))
                g.FillPolygon(cyan, new Point[] { new Point(w - 395, h), new Point(w - 350, 0), new Point(w - 340, 0), new Point(w - 385, h) });

            using (Font kicker = new Font("Segoe UI Semibold", 8F, FontStyle.Bold))
            using (Brush goldText = new SolidBrush(Livery.GoldBright))
                g.DrawString("DIGITAL DOWNFORCE SIM RACING  /  PIT WALL SYSTEM", kicker, goldText, 28, 24);
            using (Font title = new Font("Segoe UI Semibold", 28F, FontStyle.Bold))
            using (Brush text = new SolidBrush(Livery.Text))
                g.DrawString("iRacing Teammate", title, text, 24, 48);
            using (Font subtitle = new Font("Segoe UI", 10F))
            using (Brush muted = new SolidBrush(Livery.Muted))
                g.DrawString("Your software. One grid. Zero pre-race hassle.", subtitle, muted, 29, 103);

            using (Pen line = new Pen(Livery.Gold, 3F)) g.DrawLine(line, 29, 133, 78, 133);
            using (Font version = new Font("Segoe UI Semibold", 7.5F, FontStyle.Bold))
            using (Brush silverText = new SolidBrush(Livery.Silver))
                g.DrawString("TEAMMATE FOR YOUR SOFTWARE", version, silverText, 91, 127);
        }
    }

    public static class BrandAsset
    {
        private static Image logo;
        private static Image banner;

        private static Image Load(string resourceName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null) return null;
                using (Image source = System.Drawing.Image.FromStream(stream))
                {
                    return new Bitmap(source);
                }
            }
        }

        private static Image Logo
        {
            get { if (logo == null) logo = Load("DdsLogo"); return logo; }
        }

        private static Image Banner
        {
            get { if (banner == null) banner = Load("DdsBanner"); return banner; }
        }

        public static void DrawLogo(Graphics graphics, Rectangle bounds)
        {
            Image image = Logo;
            if (image == null) return;
            Rectangle source = new Rectangle(30, 285, 740, 300);
            float scale = Math.Min((float)bounds.Width / source.Width, (float)bounds.Height / source.Height);
            int width = (int)(source.Width * scale);
            int height = (int)(source.Height * scale);
            int left = bounds.Left + (bounds.Width - width) / 2;
            int top = bounds.Top + (bounds.Height - height) / 2;
            graphics.DrawImage(image, new Rectangle(left, top, width, height), source, GraphicsUnit.Pixel);
        }

        public static void DrawIcon(Graphics graphics, Rectangle bounds)
        {
            Image image = Logo;
            if (image == null) return;
            Rectangle source = new Rectangle(55, 300, 340, 265);
            graphics.DrawImage(image, bounds, source, GraphicsUnit.Pixel);
        }

        public static void DrawBanner(Graphics graphics, Rectangle bounds)
        {
            Image image = Banner;
            if (image == null) return;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(image, bounds);
        }
    }

    public class BrandLogo : Control
    {
        public BrandLogo() { DoubleBuffered = true; BackColor = Livery.Sidebar; }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            BrandAsset.DrawLogo(e.Graphics, new Rectangle(0, 4, Width, Height - 8));
        }
    }
}
