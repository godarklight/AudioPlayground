using System;
using AudioPlayground.VFOTracker;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace AudioPlayground
{
    class MainWindow : Window
    {
        [UI] private RadioButton radioCW = null;
        [UI] private RadioButton radioAM = null;
        [UI] private RadioButton radioFM = null;
        [UI] private RadioButton radioPM = null;
        [UI] private RadioButton radioDSB = null;
        [UI] private RadioButton radioLSBFFT = null;
        [UI] private RadioButton radioUSBFFT = null;
        [UI] private RadioButton radioLSBFilter = null;
        [UI] private RadioButton radioUSBFilter = null;
        [UI] private RadioButton radioCESSB = null;
        [UI] private RadioButton radioPassthrough = null;
        [UI] private RadioButton radioPassthroughFilter = null;
        [UI] private ToggleButton toggleRun = null;
        [UI] private Button btnCW = null;
        AudioDriver ad;
        CWSink cw;
        AMSink am;
        FMSink fm;
        PMSink pm;
        DSBSink dsb;
        SSBSinkFFT ssbfft;
        SSBSinkFilter ssbfilter;
        CESSB cessb;
        Passthrough pass;
        PassthroughFilter passFilter;



        public MainWindow(AudioDriver ad, CWSink cw, AMSink am, FMSink fm, PMSink pm, DSBSink dsb, SSBSinkFFT ssbfft, SSBSinkFilter ssbfilter, CESSB cessb, Passthrough pass, PassthroughFilter passFilter) : this(new Builder("MainWindow.glade"))
        {
            this.ad = ad;
            this.cw = cw;
            this.am = am;
            this.fm = fm;
            this.pm = pm;
            this.dsb = dsb;
            this.ssbfft = ssbfft;
            this.ssbfilter = ssbfilter;
            this.cessb = cessb;
            this.pass = pass;
            this.passFilter = passFilter;
        }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            DeleteEvent += Window_DeleteEvent;
            toggleRun.Clicked += RunClicked;
            radioCW.Pressed += modeCW;
            radioAM.Pressed += modeAM;
            radioFM.Pressed += modeFM;
            radioPM.Pressed += modePM;
            radioDSB.Pressed += modeDSB;
            radioLSBFFT.Pressed += modeLSBFFT;
            radioUSBFFT.Pressed += modeUSBFFT;
            radioLSBFilter.Pressed += modeLSBFilter;
            radioUSBFilter.Pressed += modeUSBFilter;
            radioCESSB.Pressed += modeCESSB;
            radioPassthrough.Pressed += modePassthrough;
            radioPassthroughFilter.Pressed += modePassthroughFilter;
            btnCW.Pressed += CWPressed;
            btnCW.Released += CWReleased;
        }

        private void modeCW(object sender, EventArgs a)
        {
            ad.source = null;
            ad.sink = cw;
        }

        private void modeAM(object sender, EventArgs a)
        {
            ad.source = am;
            ad.sink = am;
        }

        private void modeFM(object sender, EventArgs a)
        {
            ad.source = fm;
            ad.sink = fm;
        }

        private void modePM(object sender, EventArgs a)
        {
            ad.source = pm;
            ad.sink = pm;
        }

        private void modeDSB(object sender, EventArgs a)
        {
            ad.source = dsb;
            ad.sink = dsb;
        }

        private void modeLSBFFT(object sender, EventArgs a)
        {
            ad.source = ssbfft;
            ad.sink = ssbfft;
            ssbfft.lsb = true;
        }

        private void modeUSBFFT(object sender, EventArgs a)
        {
            ad.source = ssbfft;
            ad.sink = ssbfft;
            ssbfft.lsb = false;
        }

        private void modeLSBFilter(object sender, EventArgs a)
        {
            ad.source = ssbfilter;
            ad.sink = ssbfilter;
            ssbfilter.lsb = true;
        }

        private void modeUSBFilter(object sender, EventArgs a)
        {
            ad.source = ssbfilter;
            ad.sink = ssbfilter;
            ssbfilter.lsb = false;
        }

        private void modeCESSB(object sender, EventArgs a)
        {
            ad.source = cessb;
            ad.sink = cessb;
        }
        private void modePassthrough(object sender, EventArgs a)
        {
            ad.source = pass;
            ad.sink = pass;
        }
        private void modePassthroughFilter(object sender, EventArgs a)
        {
            ad.source = passFilter;
            ad.sink = passFilter;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void RunClicked(object sender, EventArgs a)
        {

        }

        private void CWPressed(object sender, EventArgs a)
        {
            cw.enabled = true;
        }

        private void CWReleased(object sender, EventArgs a)
        {
            cw.enabled = false;
        }
    }
}
