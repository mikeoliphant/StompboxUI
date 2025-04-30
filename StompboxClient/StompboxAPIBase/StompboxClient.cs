using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Xml.Linq;

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

        public virtual bool Connected { get { return false; } }
        public float BPM { get; set; }
        public bool InClientMode { get; protected set; }
        public bool AllowMidiMapping { get; set; }
        public string PluginPath { get; set; }
        public float MaxDSPLoad { get; private set; }
        public float MinDSPLoad { get; private set; }
        public List<MidiCCMapEntry> MidiCCMap { get; private set; } = new List<MidiCCMapEntry>();
        public int MidiModeCC { get; set; } = -1;
        public Dictionary<int, int> MidiStompCCMap { get; private set; } = new Dictionary<int, int>();
        public bool StopSimulateAudio { get; set; }
        public List<string> PresetNames { get; private set; }

        public IEnumerable<IAudioPlugin> AllActivePlugins
        {
            get
            {
                yield break;
            }
        }

        public virtual int CurrentPresetIndex { get; set; } = -1;

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
        }

        public virtual void Connect(string serverName, int port, Action<bool> connectCallback)
        {
        }

        public virtual void UpdatePresets()
        {
        }

        public virtual void UpdateProgram()
        {
            UpdatePresets();
        }

        public virtual void SaveCurrentPreset()
        {

        }

        public virtual void SavePresetAs(string presetName)
        {

        }

        public virtual void DeleteCurrentPreset()
        {
           
        }

        public virtual string GetGlobalChain()
        {
            throw new NotImplementedException();
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

        public virtual void SetChain(string name, List<IAudioPlugin> plugins)
        {

        }

        public virtual List<IAudioPlugin> GetChain(string name)
        {
            throw new NotImplementedException();
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

        public virtual void UpdateChain(string chainName, List<IAudioPlugin> plugins)
        {
            string cmd = "SetChain " + chainName;

            foreach (IAudioPlugin plugin in plugins)
            {
                cmd += " " + plugin.ID;
            }

            StompboxClient.Instance.SendCommand(cmd);
        }

        public virtual void Init(float sampleRate)
        {
        }

        void HandleMidi(int midiCommand, int midiData1, int midiData2)
        {
            if (MidiCallback != null)
                MidiCallback(midiCommand, midiData1, midiData2);
        }

    }
}
