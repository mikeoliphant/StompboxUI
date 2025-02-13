using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StompboxAPI
{
    class NativeAPI
    {
        public const string STOMPBOX_LIB_NAME = "stompbox-capi";


        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr CreateProcessor([MarshalAs(UnmanagedType.LPWStr)] string dataPath, bool dawMode);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void DeleteProcessor(IntPtr processor);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void Init(IntPtr processor, double sampleRate);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void StartServer(IntPtr processor);

        [DllImport(STOMPBOX_LIB_NAME)]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public static extern string GetDataPath(IntPtr processor);
    }
}
