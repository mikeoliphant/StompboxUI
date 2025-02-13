using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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

        public void Init(double sampleRate)
        {
            NativeApi.InitProcessor(nativeProcessor, sampleRate);
        }

        public UnmanagedAudioPlugin CreatePlugin(string id)
        {
            IntPtr nativePlugin = NativeApi.CreatePlugin(nativeProcessor, id);

            if (nativePlugin == IntPtr.Zero)
                return null;

            UnmanagedAudioPlugin newPlugin = new UnmanagedAudioPlugin();

            newPlugin.SetNativePlugin(nativePlugin);

            return newPlugin;
        }

        public List<string> GetAllPlugins()
        {
            return NativeApi.GetListFromStringVector(NativeApi.GetAllPlugins(nativeProcessor));
        }

        public string GetPluginSlot(string slotName)
        {
            return Marshal.PtrToStringAnsi(NativeApi.GetPluginSlot(nativeProcessor, slotName));
        }

        public List<string> GetInputChainPlugins()
        {
            return NativeApi.GetListFromStringVector(NativeApi.GetChainPlugins(nativeProcessor, "InputChain"));
        }

        public List<string> GetFxLoopPlugins()
        {
            return NativeApi.GetListFromStringVector(NativeApi.GetChainPlugins(nativeProcessor, "FxLoop"));
        }

        public List<string> GetOutputChainPlugins()
        {
            return NativeApi.GetListFromStringVector(NativeApi.GetChainPlugins(nativeProcessor, "OutputChain"));
        }

        public unsafe void Process(double* input, double* output, uint bufferSize)
        {
            NativeApi.Process(nativeProcessor, input, output, bufferSize);
        }
    }
}
