using System;
using AudioPlugSharpHost;

namespace Stompbox
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            WindowsFormsHost<StompboxPlugin> host = new WindowsFormsHost<StompboxPlugin>(new StompboxPlugin());

            host.Run();
        }
    }
}