using System.Collections.Generic;

namespace Stompbox
{
    public class SysExCC
    {
        public int SysExID { get; set; }
        public int Controller { get; set; }
        public int MaxValue { get; set; }

        public SysExCC(int sysExID, int controller, int maxValue)
        {
            this.SysExID = sysExID;
            this.Controller = controller;
            this.MaxValue = maxValue;
        }
    }

    public class SysExMapper
    {
        public byte[] SysExHeader { get; set; }

        Dictionary<int, SysExCC> ccMap = new Dictionary<int, SysExCC>();

        public SysExMapper()
        {
            SysExHeader = new byte[] { 0xF0, 0x41, 0x00, 0x00, 0x00, 0x00, 0x30, 0x12, 0x60, 0x00, 0x06 };
        }

        public void AddCC(SysExCC cc)
        {
            ccMap[cc.SysExID] = cc;
        }

        public bool HandleMidiMessage(byte[] message, out int controller, out double value)
        {
            controller = 0;
            value = 0;

            if (message.Length >= (SysExHeader.Length + 2))
            {
                for (int i = 0; i < SysExHeader.Length; i++)
                {
                    if (message[i] != SysExHeader[i])
                        return false;
                }

                int sysExID = message[SysExHeader.Length];
                int sysExValue = message[SysExHeader.Length + 1];

                if (!ccMap.ContainsKey(sysExID))
                    return false;

                SysExCC cc = ccMap[sysExID];

                controller = cc.Controller;
                value = (double)sysExValue / (double)cc.MaxValue;

                return true;
            }

            return false;
        }
    }
}
