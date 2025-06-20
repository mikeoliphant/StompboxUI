﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPlugins
{
    public class BassDrivePlugin : MiniPlugin
    {
        public BassDrivePlugin()
        {
            Company = "Nostatic Software";
            Website = "nostatic.org";
            Contact = "contact@nostatic.org";
            PluginName = "Bass Drive";
            PluginCategory = "Fx";
            PluginVersion = "0.0.1";

            PluginID = 0x58C1ABE4972C4EC8;

            EditorWidth = 680;
            EditorHeight = 400;
        }

        public override void Initialize()
        {
            base.Initialize();

            StompboxClient.SendCommand("SetGlobalChain MasterChain MasterIn SplitChain Drive Chain Clean MasterChain MasterOut");

            StompboxClient.UpdateProgram();

            StompboxClient.SendCommand("SetChain MasterIn Input");
            StompboxClient.SendCommand("SetChain MasterOut Master");

            StompboxClient.SendCommand("SetChain Drive HighLow NoiseGate NAM Cabinet");
            StompboxClient.SendCommand("SetChain Clean HighLow_2 Compressor");
        }
    }
}
