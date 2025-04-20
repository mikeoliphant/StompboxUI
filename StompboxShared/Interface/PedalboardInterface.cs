using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UILayout;

namespace Stompbox
{
    public class PedalboardInterface : InterfaceBase
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

        protected override void DrawContents()
        {
            base.DrawContents();

            float desiredScale = (Layout.Current as MonoGameLayout).UnscaledBounds.Width / 640.0f;

            if (StompboxLayout.Instance.Scale != desiredScale)
                StompboxLayout.Instance.Scale = desiredScale;

            if (StompboxClient.Instance.NeedUIReload)
            {
                UpdateUI();

                StompboxClient.Instance.NeedUIReload = false;
            }
        }

        public void UpdateUI()
        {
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
                bool addedStomp = false;

                if (ccMap != null)
                {
                    IAudioPlugin plugin = StompboxClient.Instance.AllActivePlugins.Where(p => (p.ID == ccMap.PluginName)).FirstOrDefault();

                    if (plugin != null)
                    {
                        PluginParameter parameter = plugin.Parameters.Where(p => (p.Name == ccMap.PluginParameter)).FirstOrDefault();

                        if (((parameter != null) && (parameter.ParameterType == EParameterType.Bool)) || (ccMap.PluginParameter == "Enabled"))
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

                            dock.Children.Add(GetStack(plugin, ccMap.PluginParameter));
                            dock.Children.Add(new UIElement()
                            {
                                HorizontalAlignment = EHorizontalAlignment.Stretch,
                                VerticalAlignment = EVerticalAlignment.Stretch,
                                BackgroundColor = new UIColor(0, 0, 0, 200)
                            });

                            button.SetElements(GetStack(plugin, ccMap.PluginParameter), dock);

                            if (parameter == null)  // "Enabled"
                            {
                                button.SetPressed(plugin.Enabled);

                                button.ClickAction = delegate
                                {
                                    plugin.Enabled = !plugin.Enabled;
                                };

                                plugin.SetEnabled = delegate (bool enabled)
                                {
                                    button.SetPressed(enabled);
                                };
                            }
                            else
                            {
                                button.SetPressed(parameter.Value == 1.0f);

                                button.ClickAction = delegate
                                {
                                    parameter.Value = 1.0f - parameter.Value;
                                };

                                parameter.SetValue = delegate (float value)
                                {
                                    button.SetPressed(value == 1.0f);
                                };
                            }

                            stompStack.Children.Add(button);

                            addedStomp = true;
                        }
                    }
                }

                if (!addedStomp)
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

        public override bool HandleTouch(in Touch touch)
        {
            if (!base.HandleTouch(touch))
            {
                if (IsDoubleTap(touch))
                {
                    Layout.Current.ShowPopup(new ExtraOptionsMenu(EStompboxInterfaceType.Pedalboard), ContentBounds.Center);
                }
            }

            return true;
        }
    }
}
