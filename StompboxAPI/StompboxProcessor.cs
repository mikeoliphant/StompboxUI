using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
