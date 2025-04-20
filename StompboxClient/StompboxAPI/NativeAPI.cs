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
        public static extern IntPtr GetStringVectorValue(IntPtr strVec, uint index);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr CreateProcessor([MarshalAs(UnmanagedType.LPWStr)] string dataPath, bool dawMode);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void DeleteProcessor(IntPtr processor);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void InitProcessor(IntPtr processor, float sampleRate);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void StartServer(IntPtr processor);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static unsafe extern void Process(IntPtr processor, float* input, float* output, uint bufferSize);

        [DllImport(STOMPBOX_LIB_NAME)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsPresetLoading(IntPtr processor);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void HandleCommand(IntPtr processor, [MarshalAs(UnmanagedType.LPStr)] string cmd);

        [DllImport(STOMPBOX_LIB_NAME)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool HandleMidiCommand(IntPtr processor, int midiCommand, int midiData1, int midiData2);

        [DllImport(STOMPBOX_LIB_NAME)]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public static extern string GetDataPath(IntPtr processor);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetAllPlugins(IntPtr processor);

        [DllImport(STOMPBOX_LIB_NAME)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string GetGlobalChain(IntPtr processor);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetPluginSlot(IntPtr processor, [MarshalAs(UnmanagedType.LPStr)] string slotName);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void SetPluginSlot(IntPtr processor, [MarshalAs(UnmanagedType.LPStr)] string slotName, [MarshalAs(UnmanagedType.LPStr)] string pluginID);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern uint GetPluginVectorSize(IntPtr plugVec);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetPluginVectorValue(IntPtr plugVec, uint index);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetChainPlugins(IntPtr processor, [MarshalAs(UnmanagedType.LPStr)] string chainName);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr CreatePlugin(IntPtr processor, [MarshalAs(UnmanagedType.LPStr)] string id);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetPresets(IntPtr processor);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetCurrentPreset(IntPtr processor);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void LoadPreset(IntPtr processor, [MarshalAs(UnmanagedType.LPStr)] string presetName);

        [DllImport(STOMPBOX_LIB_NAME)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string DumpSettings(IntPtr processor);

        [DllImport(STOMPBOX_LIB_NAME)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string DumpProgram(IntPtr processor);

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
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GetPluginIsUserSelectable(IntPtr plugin);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern bool GetPluginEnabled(IntPtr plugin);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void SetPluginEnabled(IntPtr plugin, bool enabled);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern float GetOutputValue(IntPtr plugin);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern uint GetPluginNumParameters(IntPtr plugin);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetPluginParameter(IntPtr plugin, uint index);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern float GetParameterValue(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern void SetParameterValue(IntPtr parameter, float value);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetParameterName(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetParameterDescription(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern float GetParameterMinValue(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern float GetParameterMaxValue(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern float GetParameterDefaultValue(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern float GetParameterRangePower(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern int GetParameterType(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetParameterEnumValues(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        [return: MarshalAs(UnmanagedType.I1)]
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
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GetParameterIsAdvanced(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetParameterFilePath(IntPtr parameter);

        [DllImport(STOMPBOX_LIB_NAME)]
        public static extern IntPtr GetParameterDisplayFormat(IntPtr parameter);



        public static List<string> GetListFromStringVector(IntPtr strVec)
        {
            if (strVec == IntPtr.Zero)
                return null;

            List<string> list = new();

            uint size = NativeApi.GetStringVectorSize(strVec);

            for (uint i = 0; i < size; i++)
            {
                list.Add(Marshal.PtrToStringAnsi(NativeApi.GetStringVectorValue(strVec, i)));
            }

            return list;
        }
    }
}
