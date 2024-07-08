using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UILayout;

namespace Stompbox
{
    public class PedalboardInterface : Dock
    {
        TextBlock presetText;
        HorizontalStack stompStack;

        public PedalboardInterface()
        {
            BackgroundColor = UIColor.Black;

            presetText = new TextBlock("-- No Preset --")
            {
                TextColor = UIColor.White,
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Top
            };
            children.Add(presetText);

            stompStack = new HorizontalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Bottom
            };
            children.Add(stompStack);
        }

        public void UpdateUI()
        {
            StompboxGame.Instance.Scale = (Layout.Current as MonoGameLayout).UnscaledBounds.Width / 640.0f;

            if (StompboxClient.Instance.CurrentPresetIndex == -1)
            {
                presetText.Text = "-- No Preset --";
            }
            else
            {
                presetText.Text = StompboxClient.Instance.PresetNames[StompboxClient.Instance.CurrentPresetIndex];
            }

            stompStack.Children.Clear();

            MidiCCMapEntry[] stomps = new MidiCCMapEntry[3];

            foreach (var stompMap in StompboxClient.Instance.MidiStompCCMap)
            {
                foreach (MidiCCMapEntry ccMap in StompboxClient.Instance.MidiCCMap)
                {
                    if (ccMap.CCNumber == stompMap.Key)
                    {
                        stomps[stompMap.Value - 1] = ccMap;
                    }
                }
            }

            foreach (MidiCCMapEntry ccMap in stomps)
            {
                if (ccMap != null)
                {
                    NinePatchButton button = new NinePatchButton(Layout.Current.GetImage("StompOutline"))
                    {
                        HorizontalAlignment = EHorizontalAlignment.Stretch
                    };

                    button.IsToggleButton = true;

                    Dock dock = new Dock()
                    {
                        HorizontalAlignment = EHorizontalAlignment.Stretch,
                        VerticalAlignment = EVerticalAlignment.Stretch
                    };

                    IAudioPlugin plugin = StompboxClient.Instance.AllActivePlugins.Where(p => (p.ID == ccMap.PluginName)).FirstOrDefault();

                    dock.Children.Add(GetStack(plugin, ccMap.PluginParameter));
                    dock.Children.Add(new UIElement()
                    {
                        HorizontalAlignment = EHorizontalAlignment.Stretch,
                        VerticalAlignment = EVerticalAlignment.Stretch,
                        BackgroundColor = new UIColor(0, 0, 0, 200)
                    });

                    button.SetElements(GetStack(plugin, ccMap.PluginParameter), dock);

                    button.SetPressed(plugin.Enabled);

                    button.ClickAction = delegate
                    {
                        plugin.Enabled = !plugin.Enabled;
                    };

                    plugin.SetEnabled = delegate (bool enabled)
                    {
                        button.SetPressed(enabled);
                    };

                    stompStack.Children.Add(button);
                }
                else
                {
                    stompStack.Children.Add(new UIElement { HorizontalAlignment = EHorizontalAlignment.Stretch });
                }
            }

            UpdateContentLayout();
        }

        UIElement GetStack(IAudioPlugin plugin, string parameterName)
        {
            VerticalStack stack = new VerticalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch
            };

            stack.BackgroundColor = UIColor.FromHex(plugin.BackgroundColor);

            UIColor foregroundColor = UIColor.FromHex(plugin.ForegroundColor);

            stack.Children.Add(new TextBlock(plugin.Name)
            {
                TextColor = foregroundColor,
                HorizontalAlignment = EHorizontalAlignment.Center
            });

            stack.Children.Add(new TextBlock(parameterName)
            {
                TextColor = foregroundColor,
                HorizontalAlignment = EHorizontalAlignment.Center
            });

            return stack;
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            if (StompboxClient.Instance.NeedUIReload)
            {
                UpdateUI();

                StompboxClient.Instance.NeedUIReload = false;
            }
        }
    }
}
