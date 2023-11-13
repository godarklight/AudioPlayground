using System;
using Gtk;

namespace AudioPlayground
{
    class AMSink : ISink, ISource
    {
        Agc agc = new Agc();
        double phase = 0.0;
        double tuningWord = 5000.0 / 48000.0 * Math.Tau;
        double afGain = 0.1; //-20db
        double[] source;
        int sourceReadPos = 0;


        public void Read(double[] buffer)
        {
            //Setup
            if (source == null)
            {
                source = new double[buffer.Length];
            }
            //Copy
            Array.Copy(buffer, source, buffer.Length);
            agc.Process(0.8, source);
            sourceReadPos = 0;
        }

        public void Write(double[] buffer)
        {
            if (source == null || sourceReadPos == source.Length)
            {
                return;
            }
            for (int i = 0; i < buffer.Length; i++)
            {
                //Write audio
                double amGain = (source[sourceReadPos] + 1.0) / 2.0;
                buffer[i] = afGain * amGain * Math.Cos(phase);
                phase += tuningWord;
                if (phase > Math.Tau)
                {
                    phase -= Math.Tau;
                }
                sourceReadPos++;
            }
        }
    }
}