using System;
using System.Numerics;
using System.Threading;
using Gtk;

namespace AudioPlayground
{
    class SSBSinkFilter : ISink, ISource
    {
        Agc agc = new Agc();
        double phase = 0.0;
        double tuningWord = 5000.0 / 48000.0 * Math.Tau;
        double afGain = 0.1; //-20db
        double[] source;
        int sourceReadPos = 0;
        public bool lsb = false;
        //5khz setting
        WindowedSinc wslpf = new WindowedSinc(201, 0.10415, false);
        WindowedSinc wshpf = new WindowedSinc(201, 0.10415, true);

        public void Read(double[] buffer)
        {
            //Setup
            if (source == null)
            {
                source = new double[buffer.Length];
            }
            //Copy
            Array.Copy(buffer, source, buffer.Length);
            sourceReadPos = 0;
            agc.Process(0.8, source);
        }

        public void Write(double[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                double dsb = Math.Cos(phase) * source[sourceReadPos];
                sourceReadPos++;
                if (lsb)
                {
                    buffer[i] = afGain * wslpf.Filter(dsb);
                }
                else
                {
                    buffer[i] = afGain * wshpf.Filter(dsb);
                }
                //Write audio
                phase += tuningWord;
                if (phase < -Math.Tau)
                {
                    phase += Math.Tau;
                }
                if (phase > Math.Tau)
                {
                    phase -= Math.Tau;
                }
            }
        }
    }
}