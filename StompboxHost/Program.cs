using System;
using AudioPlugSharpHost;

namespace Stompbox
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //StompboxPlugin plugin = new();
            //var host = new WindowsFormsHost<StompboxPlugin>(plugin);

            MiniPlugins.AmpPlugin plugin = new();
            var host = new WindowsFormsHost<MiniPlugins.AmpPlugin>(plugin);

            host.Run();
        }
    }
}