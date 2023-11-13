using System;
using System.Numerics;
using System.Threading;
using Gtk;

namespace AudioPlayground
{
    class Passthrough : ISink, ISource
    {
        double[] source;
        int sourceReadPos;

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
        }

        public void Write(double[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = source[sourceReadPos];
                sourceReadPos++;
            }
        }
    }
}