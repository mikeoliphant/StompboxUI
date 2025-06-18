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
                if (value != CurrentPresetIndex)
                {
                    base.CurrentPresetIndex = value;

                    if (CurrentPresetIndex >= 0)
                    {
                        processor.LoadPreset(PresetNames[base.CurrentPresetIndex]);

                        needPresetLoad = true;
                    }
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

            //UpdateProgram();
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

        // FIXME: these should use direct commands

        public override void SaveCurrentPreset()
        {
            SendCommand("SavePreset " + PresetNames[CurrentPresetIndex]);
        }

        public override void SavePresetAs(string presetName)
        {
            SendCommand("SavePreset " + presetName);
            SendCommand("List Presets");
            UpdateProgram();
        }

        public override void DeleteCurrentPreset()
        {
            SendCommand("DeletePreset " + PresetNames[CurrentPresetIndex]);

            UpdateProgram();
        }

        public override string GetGlobalChain()
        {
            return processor.GetGlobalChain();
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

        public override List<IAudioPlugin> GetChain(string chainName)
        {
            List<IAudioPlugin> chain = new();

            chain.AddRange(processor.GetChainPlugins(chainName));

            foreach (IAudioPlugin plugin in chain)
            {
                PluginFactory.RegisterPlugin(plugin);
            }

            return chain;
        }

        public override string GetSlotPlugin(string slotName)
        {
            return processor.GetPluginSlot(slotName);
        }

        public override void SetSlotPlugin(string slotName, string pluginID)
        {
            processor.SetPluginSlot(slotName, pluginID);
        }

        public override void Init(float sampleRate)
        {
            processor.Init(sampleRate);
        }

        public void SetMaxAudioBufferSize(uint numSamples)
        {
            processor.SetMaxAudioBufferSize(numSamples);
        }

        public String GetProgramState()
        {
            String settingsString = processor.DumpSettings();
            String programString = processor.DumpProgram();

            return settingsString + programString;
        }

        public unsafe void Process(float* input, float* output, uint bufferSize)
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

            float* inBuf = (float*)Marshal.AllocHGlobal(bufferSize * sizeof(float));
            float* outBuf = (float*)Marshal.AllocHGlobal(bufferSize * sizeof(float));

            while (true)
            {
                if (StopSimulateAudio)
                    break;

                for (int i = 0; i < bufferSize; i++)
                {
                    inBuf[i] = 0;

                    inBuf[i] += (float)Math.Sin(((float)samplePos / (float)sampleRate) * 440 * Math.PI * 2) * 0.25f;

                    samplePos++;
                }

                processor.Process(inBuf, outBuf, (uint)bufferSize);

                Thread.Sleep(sleepMS);
            }
        }
    }
}
