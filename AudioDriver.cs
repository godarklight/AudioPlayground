using System;
using Gtk;

namespace AudioPlayground
{
    using System;
    using System.Collections.Concurrent;
    using PortAudioSharp;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks.Dataflow;

    namespace VFOTracker
    {
        class AudioDriver
        {
            Stream audioStream;
            double[] sourceBuffer = new double[16];
            double[] sinkBuffer = new double[16];
            int sourceWritePos = 0;
            int sinkReadPos = 0;
            AutoResetEvent okRead = new AutoResetEvent(false);
            public ISource source;
            public ISink sink;

            public AudioDriver()
            {
                Console.WriteLine("Audio init");
                PortAudio.Initialize();
                StreamParameters inParam = new StreamParameters();
                inParam.channelCount = 1;
                inParam.device = PortAudio.DefaultInputDevice;
                inParam.sampleFormat = SampleFormat.Float32;
                inParam.suggestedLatency = 0.02;
                StreamParameters outParam = new StreamParameters();
                outParam.channelCount = 1;
                outParam.device = PortAudio.DefaultOutputDevice;
                outParam.sampleFormat = SampleFormat.Float32;
                outParam.suggestedLatency = 0.02;
                audioStream = new Stream(inParam, outParam, 48000, 0, StreamFlags.NoFlag, AudioCallback, null);
                audioStream.Start();
            }

            public StreamCallbackResult AudioCallback(IntPtr input, IntPtr output, uint frameCount, ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, IntPtr userDataPtr)
            {
                unsafe
                {
                    float* floatIn = (float*)input.ToPointer();
                    float* floatOut = (float*)output.ToPointer();
                    for (int i = 0; i < frameCount; i++)
                    {
                        sourceBuffer[sourceWritePos] = *floatIn;
                        floatIn++;
                        *floatOut = (float)sinkBuffer[sinkReadPos];
                        floatOut++;
                        sourceWritePos++;
                        sinkReadPos++;
                        if (sourceWritePos == sourceBuffer.Length)
                        {
                            if (source != null)
                            {
                                source.Read(sourceBuffer);
                            }
                            sourceWritePos = 0;
                        }
                        if (sinkReadPos == sinkBuffer.Length)
                        {
                            if (sink != null)
                            {
                                sink.Write(sinkBuffer);
                            }
                            sinkReadPos = 0;
                        }
                    }
                }
                return StreamCallbackResult.Continue;
            }

            public void Stop()
            {
                audioStream.Stop();
                PortAudio.Terminate();
            }
        }
    }
}
