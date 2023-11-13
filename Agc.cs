using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace AudioPlayground
{
    class Agc
    {
        double agcMin = 0.01;
        double agcMax = 10;

        double maxValue = 0;
        double agcBeta = 0.00001;
        double agcMultiplySlow = 0;



        public void Process(double target, double[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] > maxValue)
                {
                    maxValue = data[i];
                }
            }
            maxValue = maxValue * (1 - agcBeta);
            double agcMultiply = target / maxValue;


            if (agcMultiply > agcMax)
            {
                agcMultiply = agcMax;
            }
            if (agcMultiply < agcMin)
            {
                agcMultiply = agcMin;
            }
            for (int i = 0; i < data.Length; i++)
            {
                agcMultiplySlow = ((1.0 - agcBeta) * agcMultiplySlow) + (agcMultiply * agcBeta);
                data[i] = data[i] * agcMultiplySlow;
            }
        }
    }
}