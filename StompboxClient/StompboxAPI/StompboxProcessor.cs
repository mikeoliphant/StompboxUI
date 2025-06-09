using System;
using System.Runtime.InteropServices;

namespace StompboxAPI
{
    public class StompboxProcessor
    {
        IntPtr nativeProcessor;

        public string DataPath { get { return NativeApi.GetDataPath(nativeProcessor); } }

        public StompboxProcessor(string dataPath, bool dawMode)
        {
            nativeProcessor = NativeApi.CreateProcessor(dataPath, dawMode);
        }

        public void Init(float sampleRate)
        {
            NativeApi.InitProcessor(nativeProcessor, sampleRate);
        }

        public UnmanagedAudioPlugin CreatePlugin(string id)
        {
            return CreatePluginFromNative(NativeApi.CreatePlugin(nativeProcessor, id));

        }

        public bool IsPresetLoading { get { return NativeApi.IsPresetLoading(nativeProcessor); } }

        public void StartServer()
        {
            NativeApi.StartServer(nativeProcessor);
        }

        public void HandleCommand(string command)
        {
            NativeApi.HandleCommand(nativeProcessor, command);
        }

        public bool HandleMidiCommand(int midiCommand, int midiData1, int midiData2)
        {
            return NativeApi.HandleMidiCommand(nativeProcessor, midiCommand, midiData1, midiData2);
        }

        public List<string> GetAllPlugins()
        {
            return NativeApi.GetListFromStringVector(NativeApi.GetAllPlugins(nativeProcessor));
        }

        public string GetGlobalChain()
        {
            return NativeApi.GetGlobalChain(nativeProcessor);
        }

        public string GetPluginSlot(string slotName)
        {
            return Marshal.PtrToStringAnsi(NativeApi.GetPluginSlot(nativeProcessor, slotName));
        }

        public void SetPluginSlot(string slotName, string pluginID)
        {
            NativeApi.SetPluginSlot(nativeProcessor, slotName, pluginID);
        }

        public List<UnmanagedAudioPlugin> GetChainPlugins(string chainName)
        {
            return GetChainPlugins(NativeApi.GetChainPlugins(nativeProcessor, chainName));
        }

        public List<string> GetPresets()
        {
            return NativeApi.GetListFromStringVector(NativeApi.GetPresets(nativeProcessor));
        }

        public string GetCurrentPreset()
        {
            IntPtr preset = NativeApi.GetCurrentPreset(nativeProcessor);

            if (preset == IntPtr.Zero)
                return null;

            return Marshal.PtrToStringAnsi(preset);
        }

        public void LoadPreset(string presetName)
        {
            NativeApi.LoadPreset(nativeProcessor, presetName);
        }

        public string DumpSettings()
        {
            return NativeApi.DumpSettings(nativeProcessor);
        }

        public string DumpProgram()
        {
            return NativeApi.DumpProgram(nativeProcessor);
        }

        public unsafe void Process(float* input, float* output, uint bufferSize)
        {
            NativeApi.Process(nativeProcessor, input, output, bufferSize);
        }

        UnmanagedAudioPlugin CreatePluginFromNative(IntPtr nativePtr)
        {
            if (nativePtr == IntPtr.Zero)
                return null;

            UnmanagedAudioPlugin newPlugin = new UnmanagedAudioPlugin();

            newPlugin.SetNativePlugin(nativePtr);

            return newPlugin;
        }

        List<UnmanagedAudioPlugin> GetChainPlugins(IntPtr plugVec)
        {
            if (plugVec == IntPtr.Zero)
                return null;

            List<UnmanagedAudioPlugin> list = new();

            uint size = NativeApi.GetPluginVectorSize(plugVec);

            for (uint i = 0; i < size; i++)
            {
                list.Add(CreatePluginFromNative(NativeApi.GetPluginVectorValue(plugVec, i)));
            }

            return list;
        }
    }
}
