using System;
using Cairo;

namespace AudioPlayground
{
    class WindowedSinc
    {
        double[] filter;
        double[] history;
        int historyWritePos = 0;
        public WindowedSinc(int points, double cutoff, bool highpass)
        {
            history = new double[points];
            GenerateFilter(points, cutoff);
            if (highpass)
            {
                InvertFilter();
            }
        }

        private void GenerateFilter(int points, double cutoff)
        {
            filter = new double[points];

            for (int i = 0; i < points; i++)
            {
                int sincIndex = i - ((points - 1) / 2);
                if (sincIndex == 0)
                {
                    filter[i] = Math.Tau * cutoff;
                }
                else
                {
                    double window = 0.42 - 0.5 * Math.Cos(Math.Tau * i / (double)(points - 1)) + 0.08 * Math.Cos(2 * Math.Tau * i / (double)(points - 1));
                    filter[i] = Math.Sin(Math.Tau * cutoff * (double)sincIndex) / ((double)sincIndex) * window;
                }
            }
            Normalize();
        }

        private void Normalize()
        {
            double total = 0;
            for (int i = 0; i < filter.Length; i++)
            {
                total += filter[i];
            }
            for (int i = 0; i < filter.Length; i++)
            {
                filter[i] = filter[i] / total;
            }
        }

        private void InvertFilter()
        {
            for (int i = 0; i < filter.Length; i++)
            {
                filter[i] = -filter[i];
            }
            filter[filter.Length / 2] = filter[filter.Length / 2] + 1.0;
        }

        public double Filter(double input)
        {
            history[historyWritePos] = input;
            int historyOffset = historyWritePos;
            double total = 0;
            for (int i = 0; i < filter.Length; i++)
            {
                total += filter[i] * history[historyOffset];
                historyOffset--;
                if (historyOffset == -1)
                {
                    historyOffset += history.Length;
                }
            }
            historyWritePos++;
            if (historyWritePos == history.Length)
            {
                historyWritePos = 0;
            }
            return total;
        }
    }
}