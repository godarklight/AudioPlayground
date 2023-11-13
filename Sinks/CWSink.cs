using System;
using Gtk;

namespace AudioPlayground
{
    class CWSink : ISink
    {
        double phase = 0.0;
        double tuningWord = 700.0 / 48000.0 * Math.Tau;
        double afGain = 0.1; //-20db
        public bool enabled = false;
        double ramp = 0;
        //4ms Ramp
        double rampRate = (1.0 / 0.004) * (1.0 / 48000.0);

        public void Write(double[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                //Control ramp
                if (enabled)
                {
                    ramp += rampRate;
                    if (ramp > 1.0)
                    {
                        ramp = 1.0;
                    }
                }
                else
                {
                    ramp -= rampRate;
                    if (ramp < 0.0)
                    {
                        ramp = 0;
                    }
                }
                //Write audio
                buffer[i] = ramp * afGain * Math.Cos(phase);
                phase += tuningWord;
                if (phase > Math.Tau)
                {
                    phase -= Math.Tau;
                }
            }
        }
    }
}