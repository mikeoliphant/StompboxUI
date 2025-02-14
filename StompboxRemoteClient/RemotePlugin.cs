using System;
using Stompbox;

namespace StompboxAPI
{
    public class RemoteParameter : PluginParameter
    {
        public override double Value
        {
            get => base.Value;

            set
            {
                if (SetValue != null)
                    SetValue(value);

                base.Value = value;

                //if (Plugin.StompboxClient.InClientMode && !Plugin.StompboxClient.SuppressCommandUpdates)
                //{
                //    if (CanSyncToHostBPM && (HostBPMSyncNumerator != 0) && (HostBPMSyncDenominator != 0))
                //    {
                //        Plugin.StompboxClient.SendCommand("SetParam " + Plugin.ID + " " + Name + " " + HostBPMSyncNumerator + "/" + HostBPMSyncDenominator);
                //    }
                //    else
                //    {
                //        Plugin.StompboxClient.SendCommand("SetParam " + Plugin.ID + " " + Name + " " +
                //            (((ParameterType == EParameterType.Enum) || (ParameterType == EParameterType.File)) ? ((EnumValues.Length > 0) ? EnumValues[(int)Value] : "") : Value.ToString()));
                //    }
                //}
            }
        }
    }

    public class RemotePlugin : AudioPluginBase
    {
        public override bool Enabled
        {
            get => base.Enabled;

            set
            {
                base.Enabled = value;

                //if (StompboxClient.InClientMode && !StompboxClient.SuppressCommandUpdates)
                //{
                //    StompboxClient.SendCommand("SetParam " + ID + " Enabled " + (enabled ? "1" : "0"));
                //}
            }
        }
    }
}