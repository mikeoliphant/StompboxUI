using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Stompbox
{
    public class MidiCCMapEntry
    {
        public int CCNumber { get; set; }
        public string PluginName { get; set; }
        public string PluginParameter { get; set; }
    }

    public class StompboxClient
    {
        public static StompboxClient Instance { get; private set; }

        public static Action<string> DebugAction { get; set; }

        public PluginFactory PluginFactory { get; set; }
        public Action<int, int, int> MidiCallback { get; set; }

        public double BPM { get; set; }
        public bool InClientMode { get; protected set; }
        public bool AllowMidiMapping { get; set; }
        public string PluginPath { get; set; }
        public List<IAudioPlugin> InputPlugins { get; set; }
        public List<IAudioPlugin> FxLoopPlugins { get; set; }
        public List<IAudioPlugin> OutputPlugins { get; set; }
        public IEnumerable<IAudioPlugin> AllActivePlugins
        {
            get
            {
                yield return Tuner;

                yield return InputGain;

                if (Amp != null)
                    yield return Amp;

                foreach (IAudioPlugin plugin in InputPlugins)
                    yield return plugin;

                if (Tonestack != null)
                    yield return Tonestack;

                foreach (IAudioPlugin plugin in FxLoopPlugins)
                    yield return plugin;

                if (Cabinet != null)
                    yield return Cabinet;

                foreach (IAudioPlugin plugin in OutputPlugins)
                    yield return plugin;

                yield return AudioPlayer;

                yield return AudioRecorder;

                yield return MasterVolume;
            }
        }
        public IAudioPlugin Tuner { get; private set; }
        public IAudioPlugin InputGain { get; private set; }
        public IAudioPlugin MasterVolume { get; private set; }
        public IAudioPlugin Amp { get; private set; }
        public IAudioPlugin Tonestack { get; private set; }
        public IAudioPlugin Cabinet { get; private set; }
        public IAudioPlugin AudioPlayer { get; private set; }
        public IAudioPlugin AudioRecorder { get; private set; }
        public float MaxDSPLoad { get; private set; }
        public float MinDSPLoad { get; private set; }
        public List<MidiCCMapEntry> MidiCCMap { get; private set; } = new List<MidiCCMapEntry>();
        public int MidiModeCC { get; set; } = -1;
        public Dictionary<int, int> MidiStompCCMap { get; private set; } = new Dictionary<int, int>();
        public bool StopSimulateAudio { get; set; }
        public List<string> PresetNames { get; private set; }

        public virtual int CurrentPresetIndex { get; set;  }

        public virtual bool NeedUIReload { get; set; }

        bool needUIReload = false;

        public StompboxClient()
        {
            Instance = this;

            Debug("Creating StompboxClient.");

            AllowMidiMapping = true;
            BPM = 120;

            PluginPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "stompbox");

            PluginFactory = new PluginFactory(this);

            PresetNames = new List<string>();

            InputPlugins = new List<IAudioPlugin>();
            FxLoopPlugins = new List<IAudioPlugin>();
            OutputPlugins = new List<IAudioPlugin>();
        }

        public virtual void UpdatePresets()
        {
        }

        public virtual void UpdateProgram()
        {
            UpdatePresets();
        }


        public virtual IEnumerable<String> GetAllPluginNames()
        {
            return null;
        }

        public IEnumerable<IAudioPlugin> GetAllPluginDefinitions()
        {
            foreach (string name in GetAllPluginNames())
            {
                yield return GetPluginDefinition(name);
            }
        }

        public IEnumerable<IAudioPlugin> GetAllUserPluginDefinitions()
        {
            foreach (string name in GetAllPluginNames())
            {
                IAudioPlugin plugin = GetPluginDefinition(name);

                if (plugin.IsUserSelectable)
                    yield return plugin;
            }
        }

        public virtual IAudioPlugin GetPluginDefinition(string pluginName)
        {
            return null;
        }

        public virtual IAudioPlugin CreatePlugin(string pluginName, string pluginID)
        {
            return null;
        }

        public virtual void SendCommand(string command)
        {
        }

        public void Debug(string debugStr)
        {
            if (DebugAction != null)
                DebugAction(debugStr);
        }


        public void ReportDSPLoad(float maxDSPLoad, float minDSPLoad)
        {
            MaxDSPLoad = maxDSPLoad;
            MinDSPLoad = minDSPLoad;
        }

        public void SetInputChain(List<IAudioPlugin> plugins)
        {
            InputPlugins = plugins;
        }

        public void SetFxLoop(List<IAudioPlugin> plugins)
        {
            FxLoopPlugins = plugins;
        }


        public void SetOutputChain(List<IAudioPlugin> plugins)
        {
            OutputPlugins = plugins;
        }

        public void SetPresetNames(List<string> presetNames)
        {
            PresetNames.Clear();

            foreach (string presetName in presetNames)
            {
                PresetNames.Add(presetName);
            }
        }

        public void SetSelectedPreset(string presetName)
        {
            Debug("Set selected preset: " + presetName);

            CurrentPresetIndex = PresetNames.IndexOf(presetName);
        }

        public virtual void UpdateUI()
        {
            Debug("*** Update UI");

            Tuner = PluginFactory.CreatePlugin("Tuner");

            InputGain = PluginFactory.CreatePlugin("Input");

            Amp = CreateSlotPlugin("Amp", "NAM");

            Tonestack = CreateSlotPlugin("Tonestack", "EQ-7");

            Cabinet = CreateSlotPlugin("Cabinet", "Cabinet");

            MasterVolume = PluginFactory.CreatePlugin("Master");

            AudioPlayer = PluginFactory.CreatePlugin("AudioFilePlayer");

            if (StompboxClient.Instance.InClientMode)
                AudioRecorder = PluginFactory.CreatePlugin("AudioFileRecorder");

            NeedUIReload = true;
        }

        protected virtual IAudioPlugin CreateSlotPlugin(string slotName, string defaultPlugin)
        {
            string pluginID = GetSlotPlugin(slotName);

            if (pluginID == null)
            {
                pluginID = defaultPlugin;
            }

            SetSlotPlugin(slotName, pluginID);

            return PluginFactory.CreatePlugin(pluginID);
        }

        public virtual void SetSlotPlugin(string slotName, string pluginID)
        {

        }

        public virtual string GetSlotPlugin(string slotName)
        {
            return null;
        }

        public virtual void Init(double sampleRate)
        {
        }

        void HandleMidi(int midiCommand, int midiData1, int midiData2)
        {
            if (MidiCallback != null)
                MidiCallback(midiCommand, midiData1, midiData2);
        }

    }
}
