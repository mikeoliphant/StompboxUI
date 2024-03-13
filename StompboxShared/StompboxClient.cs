﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
#if !STOMPBOXREMOTE
using UnmanagedPlugins;
#endif

namespace Stompbox
{
    public unsafe class StompboxClient : IStompboxClient
    {
        public static StompboxClient Instance { get; private set; }

        public static Action<string> DebugAction { get; set; }

        public Action<int, int, int> MidiCallback { get; set; }

        public double BPM { get; set; }
        public bool SuppressCommandUpdates { get; set; }
        public bool InClientMode { get; }
        public bool NeedUIReload { get; set; }
        public bool AllowMidiMapping { get; set; }
        public PluginFactory PluginFactory { get; set; }
        public string PluginPath { get; set; }
        public List<IAudioPlugin> InputPlugins { get; set; }
        public List<IAudioPlugin> FxLoopPlugins { get; set; }
        public List<IAudioPlugin> OutputPlugins { get; set; }
        public IAudioPlugin InputGain { get; private set; }
        public IAudioPlugin MasterVolume { get; private set; }
        public IAudioPlugin Amp { get; private set; }
        public IAudioPlugin Tonestack { get; private set; }
        public IAudioPlugin CabConvolver { get; private set; }
        public IAudioPlugin AudioRecorder { get; private set; }
        public float MaxDSPLoad { get; private set; }
        public float MinDSPLoad { get; private set; }
        public bool StopSimulateAudio { get; set; }
        public bool Connected
        {
            get
            {
                if (!InClientMode)
                    return false;

                return networkClient.Connected;
            }
        }
        public List<string> PresetNames { get; private set; }

        public int CurrentPresetIndex
        {
            get { return currentPresetIndex; }
            set
            {
                currentPresetIndex = value;

                if ((currentPresetIndex >= 0) && (currentPresetIndex < PresetNames.Count))
                {
                    if (!SuppressCommandUpdates)
                    {
                        SendCommand("LoadPreset " + PresetNames[currentPresetIndex]);

                        UpdateProgram();
                    }
                }
            }
        }

        int currentPresetIndex;
        NetworkClient networkClient;
        ProtocolClient protocolClient;

#if !STOMPBOXREMOTE
        PluginProcessorWrapper processorWrapper;
#endif

        public StompboxClient(bool inClientMode, bool inDAWMode)
        {
            Instance = this;

            Debug("Creating StompboxClient. In client mode: " + inClientMode.ToString());

            AllowMidiMapping = true;
            BPM = 120;

            PluginPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "stompbox");

            if (!Directory.Exists(PluginPath))
            {
                Directory.CreateDirectory(PluginPath);
            }

            InClientMode = inClientMode;

            PresetNames = new List<string>();

            InputPlugins = new List<IAudioPlugin>();
            FxLoopPlugins = new List<IAudioPlugin>();
            OutputPlugins = new List<IAudioPlugin>();

            PluginFactory = new PluginFactory(this);

            if (InClientMode)
            {
                protocolClient = new ProtocolClient(this);

                networkClient = new NetworkClient();
                networkClient.LineHandler = HandleCommand;
                networkClient.DebugAction = delegate (string debug)
                {
                    Debug(debug);
                };
            }
            else
            {
#if !STOMPBOXREMOTE
                Debug("Load process wrapper");

                processorWrapper = new PluginProcessorWrapper(PluginPath, inDAWMode);
                processorWrapper.SetMidiCallback(HandleMidi);

                PluginFactory.SetPlugins(processorWrapper.GetAllPlugins());

                UpdateProgram();
#endif
            }

        }

        public void StartServer()
        {
#if !STOMPBOXREMOTE
            processorWrapper.StartServer();
#endif
        }

        Action<bool> connectCallback;

        public void Connect(string serverName, int port, Action<bool> connectCallback)
        {
            if (InClientMode)
            {
                this.connectCallback = connectCallback;

                Debug("Connect to server: " + serverName);

                networkClient.Start(serverName, 24639, ConnectCallback);
            }
        }

        void ConnectCallback(bool result)
        {
            if (result)
            {
                Debug("Connected");

                networkClient.LineHandler = HandleCommand;

                RequestConfigDump();

                connectCallback(true);
            }
            else
            {
                Debug("Connect failed");

                connectCallback(false);
            }
        }


        public void UpdatePresets()
        {
#if !STOMPBOXREMOTE
            SetPresetNames(new List<String>(processorWrapper.GetPresets().Trim().Split(' ')));

            SuppressCommandUpdates = true;
            SetSelectedPreset(processorWrapper.GetCurrentPreset());
            SuppressCommandUpdates = false;
#endif
        }

        public void UpdateProgram()
        {
            if (!InClientMode)
            {
                UpdatePresets();
                UpdateUI();
            }
            else
            {
                PluginFactory.SetPlugins(protocolClient.PluginNames);

                SendCommand("Dump Program");
            }
        }

        public void RequestConfigDump()
        {
            //Thread.Sleep(500);

            SendCommand("Dump Config");
            SendCommand("List Presets");

            if (InClientMode)
                SendCommand("PluginOutputOn");

            UpdateProgram();
        }

        public IEnumerable<String> GetAllPluginNames()
        {
            if (InClientMode)
            {
                return protocolClient.PluginNames;
            }

#if STOMPBOXREMOTE
            return null;
#else
            return processorWrapper.GetAllPlugins();
#endif
        }

        public IAudioPlugin GetPluginDefinition(string pluginName)
        {
            if (InClientMode)
            {
                return protocolClient.GetPluginDefinition(pluginName);
            }
            else
            {
                return PluginFactory.GetPluginDefinition(pluginName);
            }
        }

        public IAudioPlugin CreatePlugin(string pluginName, string pluginID)
        {
            IAudioPlugin newPlugin = null;

            if (InClientMode)
            {
                newPlugin = protocolClient.CreateNewPlugin(pluginName, pluginID);
            }
            else
            {
#if !STOMPBOXREMOTE
                PluginWrapper wrapper = processorWrapper.CreatePlugin(pluginID);

                if (wrapper == null)
                    return null;

                newPlugin = new UnmanagedAudioPlugin(wrapper);
                newPlugin.StompboxClient = this;
#endif
            }

            return newPlugin;
        }

        public void ReleasePlugin(IAudioPlugin plugin)
        {
            if (InClientMode)
            {
                protocolClient.ReleasePlugin(plugin);
            }
        }

        public void Debug(string debugStr)
        {
            if (DebugAction != null)
                DebugAction(debugStr);
        }

        string[] lineSeparator = new string[] { "\r", "\n" };

        public void HandleCommand(string commandStr)
        {
            if (InClientMode)
            {
                string[] commands = commandStr.Split(lineSeparator, 0);

                foreach (string command in commands)
                {
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        //if (Dispatcher == null)
                        //{
                        //    protocolClient.HandleCommand(command);
                        //}
                        //else
                        //{
                        //    Dispatcher.Invoke(delegate
                        //    {
                        protocolClient.HandleCommand(command);
                        //    });
                        //}
                    }
                }
            }
        }

        public void ReportDSPLoad(float maxDSPLoad, float minDSPLoad)
        {
            MaxDSPLoad = maxDSPLoad;
            MinDSPLoad = minDSPLoad;
        }

        public void SendCommand(string command)
        {
            Debug("Send command: " + command);

            if (SuppressCommandUpdates)
            {
                Debug("*** Command suppressed");
            }
            else
            {
                if (InClientMode)
                {
                    if (networkClient != null)
                    {
                        if (!networkClient.Connected)
                        {
                            Debug("*** Network client not connected");
                        }
                        else
                        {
                            Debug("Actullay sending");
                            networkClient.SendData(command + "\r\n");
                        }
                    }
                }
                else
                {
#if !STOMPBOXREMOTE
                    processorWrapper.HandleCommand(command);
#endif
                }
            }
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

        public void UpdateUI()
        {
            Debug("*** Update UI");

            InputGain = PluginFactory.CreatePlugin("Input");

            Amp = CreateSlotPlugin("Amp", "NAM");

            Tonestack = CreateSlotPlugin("Tonestack", "EQ-7");

            CabConvolver = CreateSlotPlugin("Cabinet", "CabConvolver");

            MasterVolume = PluginFactory.CreatePlugin("Master");

            if (!StompboxGame.DAWMode)
                AudioRecorder = PluginFactory.CreatePlugin("AudioFileRecorder");

            if (!InClientMode)
            {
#if !STOMPBOXREMOTE
                LoadChainEffects(InputPlugins, processorWrapper.GetInputPlugins());
                LoadChainEffects(FxLoopPlugins, processorWrapper.GetFxLoopPlugins());
                LoadChainEffects(OutputPlugins, processorWrapper.GetOutputPlugins());
#endif
            }

            NeedUIReload = true;
        }

        IAudioPlugin CreateSlotPlugin(string slotName, string defaultPlugin)
        {
            string pluginID = null;

#if !STOMPBOXREMOTE
           pluginID = processorWrapper.GetPluginSlot(slotName);
#endif
            if (pluginID == null)
            {
                pluginID = defaultPlugin;

                string cmd = "SetPluginSlot " + slotName + " " + pluginID;

                StompboxClient.Instance.SendCommand(cmd);
            }

            return PluginFactory.CreatePlugin(pluginID);
        }

        void LoadChainEffects(List<IAudioPlugin> chain, IEnumerable<String> plugins)
        {
            chain.Clear();

            foreach (string pluginName in plugins)
            {
                chain.Add(PluginFactory.CreatePlugin(pluginName));
            }
        }

        public void Init(double sampleRate)
        {
#if !STOMPBOXREMOTE
            processorWrapper.Init(sampleRate);
#endif
        }

        public String GetProgramState()
        {
#if !STOMPBOXREMOTE
            String settingsString = processorWrapper.DumpSettings();
            String programString = processorWrapper.DumpProgram();

            return settingsString + programString;
#else
            return null;
#endif
        }

        void HandleMidi(int midiCommand, int midiData1, int midiData2)
        {
            if (MidiCallback != null)
                MidiCallback(midiCommand, midiData1, midiData2);
        }

        public void Process(double* input, double* output, uint bufferSize)
        {
#if !STOMPBOXREMOTE
            processorWrapper.Process(input, output, bufferSize);
#endif
        }


        long samplePos = 0;

        public void SimulateAudio()
        {
#if !STOMPBOXREMOTE
            int bufferSize = 1024;
            int sampleRate = 44100;

            int sleepMS = sampleRate / 1000;

            Init(sampleRate);

            double* inBuf = (double *)Marshal.AllocHGlobal(bufferSize * sizeof(double));
            double* outBuf = (double*)Marshal.AllocHGlobal(bufferSize * sizeof(double));

            while (true)
            {
                if (StopSimulateAudio)
                    break;

                for (int i = 0; i < bufferSize; i++)
                {
                    inBuf[i] = 0;

                    inBuf[i] += Math.Sin(((double)samplePos / (double)sampleRate) * 440 * Math.PI * 2) * 0.25f;

                    //inBuf[i] += Math.Sin(((double)samplePos / (double)sampleRate) * 277.18 * Math.PI * 2) * 0.25f;

                    //inBuf[i] += PixGame.Random.NextDouble() * .001;

                    samplePos++;
                }

                processorWrapper.Process(inBuf, outBuf, (uint)bufferSize);

                Thread.Sleep(sleepMS);
            }
#endif
        }
    }
}
