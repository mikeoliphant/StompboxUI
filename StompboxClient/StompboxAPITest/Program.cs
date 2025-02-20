using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using StompboxAPI;

namespace StompboxAPITest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            APIClient client = new();

            client.Init(48000);

            Thread thread = new Thread(new ThreadStart(client.SimulateAudio));
            thread.Start();

            client.SetSelectedPreset("tmp");

            Thread.Sleep(1000);

            string state = client.GetProgramState();

            var tuner = client.PluginFactory.CreatePlugin("Tuner");

            tuner.Enabled = true;
        }
    }
}
