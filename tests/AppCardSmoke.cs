using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DigitalDownforceSimRacing.IRacingTeammate;

internal static class AppCardSmoke
{
    [STAThread]
    private static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        var catalog = AppCatalog.Create();
        if (catalog[0].Key != "conspit" || catalog[1].Key != "simconnectmanager")
            throw new Exception("CONSPIT and pedals must start before the other apps.");
        var definition = new AppDefinition { Key = "icon-smoke", Name = "Icon test", ProcessName = "FakeIconApp", Initials = "IT" };
        var setting = new AppSetting { Key = definition.Key, Enabled = false, Path = Path.GetFullPath(args[0]) };
        var controller = new ProcessController();
        using (var host = new Form { ClientSize = new Size(450, 378), StartPosition = FormStartPosition.Manual, Location = new Point(-30000, -30000) })
        using (var card = new AppCard(definition, setting, false))
        {
            host.Controls.Add(card);
            host.Show();
            Application.DoEvents();
            int requests = 0;
            card.LaunchRequested += delegate(object sender, EventArgs e)
            {
                if (sender != card) throw new Exception("Wrong app selected by icon click.");
                requests++;
                string error;
                if (!controller.Launch(card.Definition, card.Setting.Path, out error)) throw new Exception(error);
            };
            Button icon = card.Controls.OfType<Button>().Single(x => x.AccessibleName == "Start Icon test");
            try
            {
                card.SetLaunchEnabled(false);
                icon.PerformClick();
                if (requests != 0) throw new Exception("Disabled icon launched an app.");
                card.SetLaunchEnabled(true);
                icon.PerformClick();
                if (requests != 1 || !controller.IsManaged(definition)) throw new Exception("Icon did not launch the chosen app.");
                if (setting.Enabled) throw new Exception("Manual launch changed the Auto Mode selection.");
                icon.PerformClick();
                Process[] copies = Process.GetProcessesByName(definition.ProcessName);
                try { if (copies.Length != 1) throw new Exception("Repeated icon click created a duplicate app."); }
                finally { foreach (Process copy in copies) copy.Dispose(); }
            }
            finally { controller.StopTracked(definition); }
            host.Controls.Remove(card);
            int row = 0;
            foreach (var app in catalog.Take(2).Concat(catalog.Where(x => x.Key == "ddscoach")))
            {
                var preview = new AppCard(app, new AppSetting { Enabled = true }, true);
                preview.Location = new Point(0, row++ * 126);
                host.Controls.Add(preview);
            }
            Application.DoEvents();
            using (var bitmap = new Bitmap(host.Width, host.Height))
            {
                host.DrawToBitmap(bitmap, new Rectangle(Point.Empty, host.Size));
                bitmap.Save(args[1], System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        for (int i = 0; i < 25 && controller.IsRunning(definition); i++) Thread.Sleep(200);
        if (controller.IsRunning(definition)) throw new Exception("Icon-launched test app was not cleaned up.");
        Console.WriteLine("PASS: icon launch, disabled guard, Auto Mode preservation, duplicate prevention and hardware order.");
    }
}
