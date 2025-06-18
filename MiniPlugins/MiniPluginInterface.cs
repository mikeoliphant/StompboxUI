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

            ListUIElement currentStack = pluginStack;

            for (int pos = 0; pos < num; pos+=2)
            {
                if ((split[pos] == "Slot") || (split[pos] == "MasterSlot"))
                {
                    string slotName = split[pos + 1];

                    AddPlugin(currentStack, StompboxClient.Instance.PluginFactory.CreatePlugin(StompboxClient.Instance.GetSlotPlugin(split[pos + 1])), slotName);
                }
                else
                {
                    if (split[pos] == "SplitChain")
                    {
                        currentStack = new VerticalStack();
                        pluginStack.Children.Add(currentStack);
                    }

                    HorizontalStack hStack = new HorizontalStack();
                    currentStack.Children.Add(hStack);

                    foreach (IAudioPlugin plugin in StompboxClient.Instance.GetChain(split[pos + 1]))
                    {
                        AddPlugin(hStack, plugin, null);
                    }

                    if (split[pos] != "SplitChain")
                    {
                        currentStack = pluginStack;
                    }
                }
            }
        }

        void AddPlugin(ListUIElement stack, IAudioPlugin plugin, string slotName)
        {
            if ((plugin.Name == "Input") || (plugin.Name == "Master"))
            {
                stack.Children.Add(new GainPluginInterface(plugin) { VerticalAlignment = EVerticalAlignment.Stretch });
            }
            else
            {
                stack.Children.Add(new PluginInterface(plugin, slotName) { VerticalAlignment = EVerticalAlignment.Stretch });
            }
        }
    }
}
