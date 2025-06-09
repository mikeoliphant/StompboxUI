using Stompbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UILayout;

namespace MiniPlugins
{
    public class MiniPluginInterface : Dock
    {
        HorizontalStack pluginStack;

        public MiniPluginInterface()
        {
            pluginStack = new HorizontalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center
            };
            Children.Add(pluginStack);

            UpdateUI();
        }

        public void UpdateUI()
        {
            pluginStack.Children.Clear();

            string globalChain = StompboxClient.Instance.GetGlobalChain();

            string[] split = globalChain.Split(' ');

            int num = (split.Length / 2) * 2;

            for (int pos = 0; pos < num; pos+=2)
            {
                if ((split[pos] == "Slot") || (split[pos] == "MasterSlot"))
                {
                    string slotName = split[pos + 1];

                    AddPlugin(StompboxClient.Instance.PluginFactory.CreatePlugin(StompboxClient.Instance.GetSlotPlugin(split[pos + 1])), slotName);
                }
                else
                {
                    foreach (IAudioPlugin plugin in StompboxClient.Instance.GetChain(split[pos]))
                    {
                        AddPlugin(plugin, null);
                    }
                }
            }
        }

        void AddPlugin(IAudioPlugin plugin, string slotName)
        {
            if ((plugin.Name == "Input") || (plugin.Name == "Master"))
            {
                pluginStack.Children.Add(new GainPluginInterface(plugin) { VerticalAlignment = EVerticalAlignment.Stretch });
            }
            else
            {
                pluginStack.Children.Add(new PluginInterface(plugin, slotName) { VerticalAlignment = EVerticalAlignment.Stretch });
            }
        }
    }
}
