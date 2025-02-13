using System;
using System.Collections.Generic;
using System.Text;

namespace StompboxAPI
{
    public class RemoteClient : StompboxClient
    {
        public PluginFactory PluginFactory { get; set; }
        public bool SuppressCommandUpdates { get; set; }

        Dictionary<string, string> slotPlugins = new Dictionary<string, string>();

        NetworkClient networkClient;
        ProtocolClient protocolClient;

        public bool Connected
        {
            get
            {
                return networkClient.Connected;
            }
        }

        public override int CurrentPresetIndex
        {
            get => base.CurrentPresetIndex;
            
            set
            {
                base.CurrentPresetIndex = value;

                if (!SuppressCommandUpdates)
                {
                    SendCommand("LoadPreset " + PresetNames[currentPresetIndex]);
                }
            }
        }

        public RemoteClient()
            : base()
        {
            PluginFactory = new PluginFactory(this);

            InClientMode = true;

            protocolClient = new ProtocolClient(this);

            networkClient = new NetworkClient();
            networkClient.LineHandler = HandleCommand;
            networkClient.DebugAction = delegate (string debug)
            {
                Debug(debug);
            };
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

        public void Disconnect()
        {
            if (InClientMode)
            {
                networkClient.Stop();
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

        public override void UpdatePresets()
        {
            base.UpdatePresets();

            SendCommand("List Presets");
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

        public override IEnumerable<string> GetAllPluginNames()
        {
            return protocolClient.PluginNames;
        }

        public override IAudioPlugin GetPluginDefinition(string pluginName)
        {
            return protocolClient.GetPluginDefinition(pluginName);
        }

        public override IAudioPlugin CreatePlugin(string pluginName, string pluginID)
        {
            return protocolClient.CreateNewPlugin(pluginName, pluginID);
        }

        public void ReleasePlugin(IAudioPlugin plugin)
        {
            protocolClient.ReleasePlugin(plugin);
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
                        protocolClient.HandleCommand(command);
                    }
                }
            }
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
        }

        public void SetSlotPlugin(string slotName, string pluginID)
        {
            slotPlugins[slotName] = pluginID;
        }

        protected override IAudioPlugin CreateSlotPlugin(string slotName, string defaultPlugin)
        {
            string pluginID = null;

            slotPlugins.TryGetValue(slotName, out pluginID);

            if (pluginID == null)
            {
                pluginID = defaultPlugin;

                string cmd = "SetPluginSlot " + slotName + " " + pluginID;

                SendCommand(cmd);
            }

            return PluginFactory.CreatePlugin(pluginID);
        }
    }
}
