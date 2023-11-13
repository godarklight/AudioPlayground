using System;
using Gtk;

namespace AudioPlayground
{
    class FMSink : ISink, ISource
    {
        Agc agc = new Agc();
        double phase = 0.0;
        double tuningWord = 5000.0 / 48000.0 * Math.Tau;
        double deviationWord = 4000.0 / 48000.0 * Math.Tau;
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
                //Adjust frequency
                double adjust = deviationWord * source[i];
                phase += adjust;
                //Write audio
                buffer[i] = afGain * Math.Cos(phase);
                //Step carrier
                phase += tuningWord;
                if (phase < -Math.Tau)
                {
                    phase += Math.Tau;
                }
                if (phase > Math.Tau)
                {
                    phase -= Math.Tau;
                }
                sourceReadPos++;
            }
        }
    }
}