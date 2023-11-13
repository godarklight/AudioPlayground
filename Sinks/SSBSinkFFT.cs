using System;
using System.Numerics;
using System.Threading;
using Gtk;

namespace AudioPlayground
{
    class SSBSinkFFT : ISink, ISource
    {
        Agc agc = new Agc();
        double phase = 0.0;
        double tuningWord = 5000.0 / 48000.0 * Math.Tau;
        double afGain = 0.1; //-20db
        double[] source;
        int inWritePos = 0;
        Complex[] in1 = new Complex[1024];
        Complex[] in2 = new Complex[1024];
        Complex[] out1 = new Complex[512];
        Complex[] out2 = new Complex[512];
        int outReadPos = 0;
        public bool lsb = false;
        Thread fftThread;
        AutoResetEvent are = new AutoResetEvent(false);
        bool outputReady = false;
        public bool running = true;

        public SSBSinkFFT()
        {
            fftThread = new Thread(new ThreadStart(FFTLoop));
            fftThread.Start();
        }

        private void FFTLoop()
        {
            while (running)
            {
                if (are.WaitOne(10))
                {
                    //Hilbert transform
                    Complex[] fft = FFT.CalcFFT(in2);
                    for (int i = 1; i < fft.Length / 2; i++)
                    {
                        fft[i] = fft[i] * 2.0;
                    }
                    for (int i = fft.Length / 2; i < fft.Length; i++)
                    {
                        fft[i] = Complex.Zero;
                    }
                    //Take middle half of IFFT
                    Complex[] ifft = FFT.CalcIFFT(fft);
                    for (int i = 0; i < out2.Length; i++)
                    {
                        int quarterOffset = out2.Length / 4;
                        out2[i] = ifft[i + quarterOffset];
                    }
                    if (outputReady)
                    {
                        Console.WriteLine("SSB buffer was not consumed, overwriting");
                    }
                    outputReady = true;
                }
            }
        }


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
            for (int i = 0; i < source.Length; i++)
            {
                in1[inWritePos] = source[i];
                inWritePos++;
                if (inWritePos == in1.Length)
                {
                    Array.Copy(in1, 0, in2, 0, in1.Length);
                    //Overlap array
                    int halfLength = in1.Length / 2;
                    for (int j = 0; j < halfLength; j++)
                    {
                        in1[j] = in1[j + halfLength];
                    }
                    are.Set();
                    inWritePos = halfLength;
                }
            }
        }

        public void Write(double[] buffer)
        {
            if (outReadPos == out1.Length)
            {
                if (outputReady)
                {
                    //Swap buffer
                    Complex[] temp = out1;
                    out1 = out2;
                    out2 = temp;
                    outputReady = false;
                    outReadPos = 0;
                }
                else
                {
                    Array.Clear(buffer);
                    return;
                }
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                double real = Math.Cos(phase) * out1[outReadPos].Real;
                double imaginary = Math.Sin(phase) * out1[outReadPos].Imaginary;
                //Write audio
                if (lsb)
                {
                    buffer[i] = afGain * (real - imaginary);
                }
                else
                {
                    buffer[i] = afGain * (real + imaginary);
                }
                //Step output
                outReadPos++;
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
            }
        }
    }
}