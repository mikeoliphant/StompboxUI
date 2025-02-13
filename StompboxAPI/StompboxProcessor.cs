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

        public string DataPath { get { return NativeAPI.GetDataPath(nativeProcessor); } }

        public StompboxProcessor(string dataPath, bool dawMode)
        {
            nativeProcessor = NativeAPI.CreateProcessor(dataPath, dawMode);
        }
    }
}
