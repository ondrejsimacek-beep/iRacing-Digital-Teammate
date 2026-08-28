using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace DigitalDownforceSimRacing.IRacingTeammate
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            try { SetProcessDPIAware(); } catch { }
            bool created;
            using (Mutex mutex = new Mutex(true, "DigitalDownforceSimRacing.IRacingTeammate", out created))
            {
                if (!created)
                {
                    MessageBox.Show("iRacing Teammate is already running.", "Digital Downforce Sim Racing",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                bool screenshotMode = args.Length == 2 && args[0] == "--screenshot";
                bool startMinimized = !screenshotMode && Array.Exists(args, delegate(string argument)
                {
                    return String.Equals(argument, "--minimized", StringComparison.OrdinalIgnoreCase);
                });
                LauncherForm form = new LauncherForm(screenshotMode, startMinimized);

                if (screenshotMode)
                {
                    form.StartPosition = FormStartPosition.Manual;
                    form.Location = new Point(-30000, -30000);
                    form.Show();
                    Application.DoEvents();
                    using (Bitmap bitmap = new Bitmap(form.ClientSize.Width, form.ClientSize.Height))
                    {
                        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.ClientSize));
                        string output = Path.GetFullPath(args[1]);
                        Directory.CreateDirectory(Path.GetDirectoryName(output));
                        bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    form.Close();
                    return;
                }

                Application.Run(form);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
