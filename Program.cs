using System;
using AudioPlayground.VFOTracker;
using Gtk;

namespace AudioPlayground
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            AudioDriver ad = new AudioDriver();
            //CW
            CWSink cw = new CWSink();
            ad.sink = cw;
            AMSink am = new AMSink();
            FMSink fm = new FMSink();
            PMSink pm = new PMSink();
            DSBSink dsb = new DSBSink();
            SSBSinkFFT ssbfft = new SSBSinkFFT();
            SSBSinkFilter ssbfilter = new SSBSinkFilter();
            CESSB cessb = new CESSB();
            Passthrough pass = new Passthrough();
            PassthroughFilter passFilter = new PassthroughFilter();


            Application.Init();

            var app = new Application("org.AudioPlayground.AudioPlayground", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var win = new MainWindow(ad, cw, am, fm, pm, dsb, ssbfft, ssbfilter, cessb, pass, passFilter);
            app.AddWindow(win);

            win.Show();
            Application.Run();

            ssbfft.running = false;
            cessb.running = false;
        }
    }
}
