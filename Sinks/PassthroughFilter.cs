using System;
using System.Numerics;
using System.Threading;
using Gtk;

namespace AudioPlayground
{
    class PassthroughFilter : ISink, ISource
    {
        Complex[] fftIn = new Complex[2048];
        Complex[] fftIn2 = new Complex[2048];
        int fftPos = 0;
        Complex[] filter = new Complex[2048];
        double[] out1 = new double[1024];
        double[] out2 = new double[1024];
        int outputPos = 1024;
        bool outputReady = false;
        Thread fftThread;
        AutoResetEvent areFFT = new AutoResetEvent(false);
        public bool running = true;

        public PassthroughFilter()
        {
            //We'll do a hilbert transform for the lulz, you could also leave the negative frequencies instead.
            filter[0] = 1.0;
            for (int i = 1; i < filter.Length / 2; i++)
            {
                filter[i] = 2.0;
            }
            //Bandwidth filter
            double hzPerBin = 48000.0 / (double)filter.Length;
            int bin30 = (int)(3000.0 / hzPerBin);
            int bin33 = (int)(3300.0 / hzPerBin);
            int span = bin33 - bin30;
            for (int i = 0; i < span; i++)
            {
                double percent = i / (double)span;
                double mult = Math.Exp(-(percent * percent) * 4.0);
                filter[i + bin30] = filter[i + bin30] * mult;
            }
            /*
            int bin00 = (int)(50.0 / hzPerBin);
            int bin01 = (int)(150.0 / hzPerBin);
            int lowSpan = bin01 - bin00;
            for (int i = 0; i < lowSpan; i++)
            {
                double percent = i / (double)lowSpan;
                double mult = 1.0 - Math.Exp(-(percent * percent) * 2.0);
                filter[i + bin00] = filter[i + bin00] * mult;
            }
            //Delete everything below the filter
            for (int i = 0; i < bin00; i++)
            {
                filter[i] = Complex.Zero;
            }
            */
            //Delete everything above the filter
            for (int i = bin33; i < filter.Length / 2; i++)
            {
                filter[i] = Complex.Zero;
            }
            fftThread = new Thread(new ThreadStart(FFTThread));
            fftThread.Start();
        }

        private void FFTThread()
        {
            while (running)
            {
                if (areFFT.WaitOne(1))
                {
                    //FFT
                    Complex[] fft = FFT.CalcFFT(fftIn2);
                    //Filter
                    for (int i = 0; i < fftIn2.Length; i++)
                    {
                        fft[i] = fft[i] * filter[i];
                    }

                    //IFFT, keep middle
                    Complex[] ifft = FFT.CalcIFFT(fft);
                    int offset = ifft.Length / 4;
                    for (int i = 0; i < ifft.Length / 2; i++)
                    {
                        double sample = ifft[i + offset].Real;
                        //Awful limit test
                        /*
                        sample *= 1.414;
                        if (sample > 0.25)
                        {
                            sample = 0.25;
                        }
                        if (sample < -0.25)
                        {
                            sample = -0.25;
                        }
                        */
                        out2[i] = sample;
                    }
                    outputReady = true;
                }
            }
        }

        public void Read(double[] buffer)
        {
            //Setup
            for (int i = 0; i < buffer.Length; i++)
            {
                fftIn[fftPos] = buffer[i];
                fftPos++;
                if (fftPos == fftIn.Length)
                {
                    Complex[] temp = fftIn2;
                    fftIn2 = fftIn;
                    fftIn = temp;
                    //Save the last half of the input data for FFT overlap
                    Array.Copy(fftIn2, fftIn2.Length / 2, fftIn, 0, fftIn.Length / 2);
                    fftPos = fftIn.Length / 2;
                    areFFT.Set();
                }
            }
        }

        public void Write(double[] buffer)
        {
            if (outputPos == out1.Length)
            {
                if (outputReady)
                {
                    //Swap buffer
                    double[] temp = out1;
                    out1 = out2;
                    out2 = temp;
                    outputReady = false;
                    outputPos = 0;
                }
                else
                {
                    Array.Clear(buffer);
                    return;
                }
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                //Write audio
                buffer[i] = out1[outputPos];
                //Step output
                outputPos++;
                //Step carrier
            }
        }
    }
}