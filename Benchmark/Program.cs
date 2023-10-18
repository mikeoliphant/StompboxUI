using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnmanagedPlugins;

namespace Benchmark
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            int bufferSize = 64;
            int numPasses = 100000;

            Stopwatch stopWatch = new Stopwatch();

            double[][] mangedBuffers = new double[1][];
            mangedBuffers[0] = new double[bufferSize];

            double** unmangedBuffers = (double**)Marshal.AllocHGlobal(sizeof(double*));
            unmangedBuffers[0] = (double*)Marshal.AllocHGlobal(bufferSize * sizeof(double));

            IFaustDSP plugin = new Reverb();

            Console.WriteLine("Starting managed benchmark");

            plugin.Init(44100);

            stopWatch.Start();

            for (int i = 0; i < numPasses; i++)
            {
                plugin.Compute(bufferSize, mangedBuffers, mangedBuffers);
            }

            stopWatch.Stop();

            Console.WriteLine("Processing took " + stopWatch.Elapsed.TotalMilliseconds + "ms");

            PluginProcessorWrapper processorWrapper = new PluginProcessorWrapper(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GuitarSim"));
            PluginWrapper pluginWrapper = processorWrapper.CreatePlugin("Reverb");

            Console.WriteLine("Starting unmanaged benchmark");

            pluginWrapper.Initialize(44100);

            stopWatch.Reset();
            stopWatch.Start();

            for (int i = 0; i < numPasses; i++)
            {
                pluginWrapper.Process(unmangedBuffers, unmangedBuffers, (uint)bufferSize);
            }

            stopWatch.Stop();

            Console.WriteLine("Processing took " + stopWatch.Elapsed.TotalMilliseconds + "ms");
        }
    }
}
