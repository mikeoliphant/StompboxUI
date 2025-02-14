using System;
using System.Runtime.InteropServices;
using Stompbox;

namespace StompboxAPI
{
    public class APIClient : StompboxClient
    {
        StompboxProcessor processor;
        bool needPresetLoad = false;

        public override bool NeedUIReload
        {
            get
            {
                if (needPresetLoad && !processor.IsPresetLoading())
                {
                    UpdateProgram();

                    needPresetLoad = false;

                    return true;
                }

                return base.NeedUIReload;
            }

            set => base.NeedUIReload = value;
        }

        public override int CurrentPresetIndex
        {
            get => base.CurrentPresetIndex;

            set
            {
                base.CurrentPresetIndex = value;

                needPresetLoad = true;
            }
        }

        public APIClient()
            : base()
        {
            if (!Directory.Exists(PluginPath))
            {
                Directory.CreateDirectory(PluginPath);
            }

            processor = new StompboxProcessor(PluginPath, dawMode: true);

            InClientMode = false;

            //processorWrapper.SetMidiCallback(HandleMidi);

            UpdateProgram();
        }

        public void StartServer()
        {
            processor.StartServer();
        }

        public override void UpdatePresets()
        {
            needPresetLoad = false;

            base.UpdatePresets();

            //SetPresetNames(new List<String>(processorWrapper.GetPresets().Trim().Split(' ')));

            //SuppressCommandUpdates = true;
            //SetSelectedPreset(processorWrapper.GetCurrentPreset());
            //SuppressCommandUpdates = false;
        }

        public override void UpdateProgram()
        {
            base.UpdateProgram();

            UpdateUI();
        }

        public override IEnumerable<string> GetAllPluginNames()
        {
            return processor.GetAllPlugins();
        }

        public override IAudioPlugin GetPluginDefinition(string pluginName)
        {
            return PluginFactory.GetPluginDefinition(pluginName);
        }

        public override IAudioPlugin CreatePlugin(string pluginName, string pluginID)
        {
            return processor.CreatePlugin(pluginID);
        }

        public override void UpdateUI()
        {
            LoadChainEffects(InputPlugins, processor.GetInputChainPlugins());
            LoadChainEffects(FxLoopPlugins, processor.GetFxLoopPlugins());
            LoadChainEffects(OutputPlugins, processor.GetOutputChainPlugins());

            base.UpdateUI();
        }

        void LoadChainEffects(List<IAudioPlugin> chain, IEnumerable<String> plugins)
        {
            chain.Clear();

            foreach (string pluginName in plugins)
            {
                chain.Add(PluginFactory.CreatePlugin(pluginName));
            }
        }

        protected override IAudioPlugin CreateSlotPlugin(string slotName, string defaultPlugin)
        {
            string pluginID = processor.GetPluginSlot(slotName);

           return PluginFactory.CreatePlugin(pluginID);
        }

        public override void Init(double sampleRate)
        {
            processor.Init(sampleRate);
        }

        //        public String GetProgramState()
        //        {
        //#if !STOMPBOXREMOTE
        //            String settingsString = processorWrapper.DumpSettings();
        //            String programString = processorWrapper.DumpProgram();

        //            return settingsString + programString;
        //#else
        //            return null;
        //#endif
        //        }

        public unsafe void Process(double* input, double* output, uint bufferSize)
        {
            processor.Process(input, output, bufferSize);
        }

        long samplePos = 0;

        public unsafe void SimulateAudio()
        {
            int bufferSize = 1024;
            int sampleRate = 44100;

            int sleepMS = sampleRate / 1000;

            Init(sampleRate);

            double* inBuf = (double*)Marshal.AllocHGlobal(bufferSize * sizeof(double));
            double* outBuf = (double*)Marshal.AllocHGlobal(bufferSize * sizeof(double));

            while (true)
            {
                if (StopSimulateAudio)
                    break;

                for (int i = 0; i < bufferSize; i++)
                {
                    inBuf[i] = 0;

                    inBuf[i] += Math.Sin(((double)samplePos / (double)sampleRate) * 440 * Math.PI * 2) * 0.25f;

                    samplePos++;
                }

                processor.Process(inBuf, outBuf, (uint)bufferSize);

                Thread.Sleep(sleepMS);
            }
        }
    }
}
