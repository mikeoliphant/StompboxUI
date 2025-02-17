using System.Runtime.InteropServices;
using System.Threading;
using StompboxAPI;

namespace StompboxAPITest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Thread thread;

            StompboxProcessor processor = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "stompbox"), dawMode: true);

            thread = new Thread(new ThreadStart(delegate { SimulateAudio(processor); }));

            thread.Start();

            var pluginNames = processor.GetAllPlugins();

            UnmanagedAudioPlugin tuner = processor.CreatePlugin("Tuner");

            if (tuner != null)
            {
                tuner.Enabled = true;

                bool enabled = tuner.Enabled;

                string id = tuner.ID;
            }

            processor.SetPluginSlot("Cabinet", "Cabinet");

            string slot = processor.GetPluginSlot("Cabinet");

            string preset = processor.GetCurrentPreset();

            processor.LoadPreset("03Marshall");

            while (processor.IsPresetLoading)
            {
                
            }

            processor.LoadPreset("01Clean");

            while (processor.IsPresetLoading)
            {

            }

            var cab = processor.CreatePlugin("Cabinet");

            preset = processor.GetCurrentPreset();            
        }

        static unsafe void SimulateAudio(StompboxProcessor processor)
        {
            int bufferSize = 1024;
            int sampleRate = 44100;

            int sleepMS = sampleRate / 1000;

            processor.Init(sampleRate);

            double* inBuf = (double*)Marshal.AllocHGlobal(bufferSize * sizeof(double));
            double* outBuf = (double*)Marshal.AllocHGlobal(bufferSize * sizeof(double));

            long samplePos = 0;

            while (true)
            {
                for (int i = 0; i < bufferSize; i++)
                {
                    inBuf[i] = 0;

                    inBuf[i] += Math.Sin(((double)samplePos / (double)sampleRate) * 440 * Math.PI * 2) * 0.25f;

                    samplePos++;
                }

                processor.Process(inBuf, outBuf, (uint)bufferSize);

                Thread.Sleep(sleepMS);
            }
        }
    }
}
