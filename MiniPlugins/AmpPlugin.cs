using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPlugins
{
    public class AmpPlugin : MiniPlugin
    {
        public AmpPlugin()
        {
            Company = "Nostatic Software";
            Website = "nostatic.org";
            Contact = "contact@nostatic.org";
            PluginName = "SB-Amp";
            PluginCategory = "Fx";
            PluginVersion = "0.0.1";

            PluginID = 0x66D6F61F1DE14831;
        }

        public override void Initialize()
        {
            base.Initialize();

            StompboxClient.SendCommand("SetGlobalChain MasterChain MasterIn Slot Amp Slot Tonestack Slot Cabinet MasterChain MasterOut");

            StompboxClient.UpdateProgram();

            StompboxClient.SendCommand("SetChain MasterIn Input");
            StompboxClient.SendCommand("SetChain MasterOut Master");

            StompboxClient.SetDefaultSlotPlugin("Amp", "NAM");
            StompboxClient.SetDefaultSlotPlugin("Tonestack", "EQ-7");
            StompboxClient.SetDefaultSlotPlugin("Cabinet", "Cabinet");
        }
    }
}
