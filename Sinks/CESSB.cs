using System;
using System.Numerics;
using System.Threading;
using Gtk;

namespace AudioPlayground
{
    class CESSB : ISink, ISource
    {
        double[] source;
        int inWritePos = 0;
        Complex[] in1 = new Complex[2048];
        Complex[] in2 = new Complex[2048];
        Complex[] out1 = new Complex[1024];
        Complex[] out2 = new Complex[1024];
        Complex[] hilbertAndBW = new Complex[2048];
        Complex[] bwFilter = new Complex[2048];
        int outReadPos = 0;
        Thread fftThread;
        AutoResetEvent are = new AutoResetEvent(false);
        bool outputReady = false;
        public bool running = true;

        public CESSB()
        {
            SetupFilter();
            fftThread = new Thread(new ThreadStart(FFTLoop));
            fftThread.Start();
        }

        private void SetupFilter()
        {
            hilbertAndBW[0] = 1.0;
            bwFilter[0] = 1.0;
            for (int i = 1; i < hilbertAndBW.Length / 2; i++)
            {
                hilbertAndBW[i] = 2.0;
                bwFilter[i] = 1.0;
            }
            //Nyquist and positive is zero.
            //We'll go for rasied cosine rolloff, 3khz start of rolloff, 300hz rolloff.
            double hzPerBin = 48000.0 / (double)hilbertAndBW.Length;
            int bin30 = (int)(3000.0 / hzPerBin);
            int bin33 = (int)(3300.0 / hzPerBin);
            int span = bin33 - bin30;
            for (int i = 0; i < span; i++)
            {
                double percent = i / (double)span;
                double mult = Math.Exp(-(percent * percent) * 4.0);
                hilbertAndBW[i + bin30] = hilbertAndBW[i + bin30] * mult;
                bwFilter[i + bin30] = bwFilter[i + bin30] * mult;
            }
            //Lowpass as well
            /*
            int bin00 = (int)(50.0 / hzPerBin);
            int bin01 = (int)(150.0 / hzPerBin);
            int lowSpan = bin01 - bin00;
            for (int i = 0; i < lowSpan; i++)
            {
                double percent = i / (double)lowSpan;
                //Raised cosine, range 0.0 to 1.0
                double mult = 1.0 - Math.Exp(-(percent * percent) * 2.0);
                hilbertAndBW[i + bin00] = hilbertAndBW[i + bin00] * mult;
                bwFilter[i + bin00] = bwFilter[i + bin00] * mult;
            }
            //Delete everything below the filter
            for (int i = 0; i < bin00; i++)
            {
                hilbertAndBW[i] = Complex.Zero;
                bwFilter[i] = Complex.Zero;
            }
            */
            //Delete everything above the filter
            for (int i = bin33; i < hilbertAndBW.Length / 2; i++)
            {
                hilbertAndBW[i] = Complex.Zero;
                bwFilter[i] = Complex.Zero;
            }
        }

        private void FFTLoop()
        {
            while (running)
            {
                if (are.WaitOne(10))
                {
                    //Hilbert transform
                    Complex[] fft = FFT.CalcFFT(in2);
                    for (int i = 1; i < fft.Length; i++)
                    {
                        fft[i] = fft[i] * hilbertAndBW[i];
                    }
                    Complex[] ifft = FFT.CalcIFFT(fft);

                    //Clip to 0.25 magnitude -12dbFS
                    double LIMIT = 0.25;
                    for (int i = 0; i < ifft.Length; i++)
                    {
                        //Apply 3db gain
                        ifft[i] = ifft[i] * 1.414;
                        Complex sample = ifft[i];
                        if (sample.Magnitude > LIMIT)
                        {
                            double mult = LIMIT / sample.Magnitude;
                            ifft[i] = sample * mult;
                        }
                    }

                    //Fitler after limiting
                    Complex[] fft2 = FFT.CalcFFT(ifft);
                    for (int i = 0; i < fft2.Length; i++)
                    {
                        fft2[i] = fft2[i] * bwFilter[i];
                    }
                    Complex[] ifft2 = FFT.CalcIFFT(fft2);


                    //Take middle half of IFFT
                    int quarterOffset = ifft2.Length / 4;
                    for (int i = 0; i < out2.Length; i++)
                    {
                        out2[i] = ifft2[i + quarterOffset];
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
                //Write audio
                buffer[i] = out1[outReadPos].Real;
                //Step output
                outReadPos++;
                //Step carrier
            }
        }
    }
}