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
                if (needPresetLoad && !processor.IsPresetLoading)
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

                if (CurrentPresetIndex >= 0)
                {
                    processor.LoadPreset(PresetNames[base.CurrentPresetIndex]);

                    needPresetLoad = true;
                }
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

            SetPresetNames(processor.GetPresets());

            SetSelectedPreset(processor.GetCurrentPreset());
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

        public override void SendCommand(string command)
        {
            processor.HandleCommand(command);
        }

        public override void UpdateUI()
        {
            LoadChainEffects(InputPlugins, processor.GetInputChainPlugins());
            LoadChainEffects(FxLoopPlugins, processor.GetFxLoopPlugins());
            LoadChainEffects(OutputPlugins, processor.GetOutputChainPlugins());

            base.UpdateUI();
        }

        void LoadChainEffects(List<IAudioPlugin> chain, IEnumerable<UnmanagedAudioPlugin> plugins)
        {
            chain.Clear();

            chain.AddRange(plugins);

            // FIXME: notify pluginfactory?
        }

        public override string GetSlotPlugin(string slotName)
        {
            return processor.GetPluginSlot(slotName);
        }

        public override void SetSlotPlugin(string slotName, string pluginID)
        {
            processor.SetPluginSlot(slotName, pluginID);
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
