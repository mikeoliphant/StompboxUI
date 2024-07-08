using System;
using AudioPlugSharpHost;

namespace Stompbox
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            StompboxPlugin plugin = new StompboxPlugin();

            WindowsFormsHost<StompboxPlugin> host = new WindowsFormsHost<StompboxPlugin>(plugin);

            host.Run();
        }
    }
}