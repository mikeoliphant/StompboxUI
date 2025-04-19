using System;
using AudioPlugSharpHost;

namespace Stompbox
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            MiniPlugins.AmpPlugin plugin = new();

            var host = new WindowsFormsHost<MiniPlugins.AmpPlugin>(plugin);

            host.Run();
        }
    }
}