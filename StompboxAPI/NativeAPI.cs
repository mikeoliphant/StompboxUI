using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StompboxAPI
{
    class NativeApi
    {
        public const string STOMPBOX_LIB_NAME = "stompbox-capi";

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern uint GetStringVectorSize(IntPtr strVec);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetStringVectorValue(IntPtr strVec, int index);

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

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetAllPlugins(IntPtr processor);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr CreatePlugin(IntPtr processor, [MarshalAs(UnmanagedType.LPStr)] string id);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetPluginName(IntPtr plugin);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetPluginID(IntPtr plugin);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetPluginDescription(IntPtr plugin);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetPluginBackgroundColor(IntPtr plugin);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetPluginForegroundColor(IntPtr plugin);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern bool GetPluginIsUserSelectable(IntPtr plugin);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern bool GetPluginEnabled(IntPtr plugin);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void SetPluginEnabled(IntPtr plugin, bool enabled);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern double GetOutputValue(IntPtr plugin);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern uint GetPluginNumParameters(IntPtr plugin);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetPluginParameter(IntPtr plugin, uint index);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern double GetParameterValue(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void SetParameterValue(IntPtr parameter, double value);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetParameterName(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetParameterDescription(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern double GetParameterMinValue(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern double GetParameterMaxValue(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern double GetParameterDefaultValue(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern double GetParameterRangePower(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern int GetParameterType(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern bool GetParameterCanSyncToHostBPM(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern int GetParameterBPMSyncNumerator(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern int GetParameterBPMSyncDenominator(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void SetParameterBPMSyncNumerator(IntPtr parameter, int numerator);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void SetParameterBPMSyncDenominator(IntPtr parameter, int denom);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern bool GetParameterIsAdvanced(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetParameterFilePath(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetParameterDisplayFormat(IntPtr parameter);



        public static List<string> GetListFromStringVector(IntPtr strVec)
        {
            List<string> list = new();

            for (int i = 0; i < NativeApi.GetStringVectorSize(strVec); i++)
            {
                list.Add(Marshal.PtrToStringAnsi(NativeApi.GetStringVectorValue(strVec, i)));
            }

            return list;
        }
    }
}
