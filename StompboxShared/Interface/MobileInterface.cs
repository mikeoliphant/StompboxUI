using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UILayout;

namespace Stompbox
{
    public class MobileInterface : Dock
    {
        public static DAWInterface Instance { get; private set; }

        PluginChainDisplay inputChainDisplay;
        PluginChainDisplay fxLoopDisplay;
        PluginChainDisplay outputChainDisplay;

        Dock selectedPluginDock;
        PluginInterface currentSelectedPlugin;
        UIElementWrapper selectedPluginWrapper;

        HorizontalStack topUIStack;
        VerticalStack programStack;
        HorizontalStack ampStack;
        EnumInterface currentProgramInterface;
        TextToggleButton midiToggleButton;
        StringBuilderTextBlock dspLoadText;

        public MobileInterface()
        {
            Padding = new LayoutPadding(5);
            BackgroundColor = new UIColor(60, 60, 60);

            VerticalStack vStack = new VerticalStack
            {
                ChildSpacing = 5,
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch
            };
            Children.Add(vStack);

            topUIStack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Stretch };
            vStack.Children.Add(topUIStack);

            programStack = new VerticalStack { HorizontalAlignment = EHorizontalAlignment.Stretch, VerticalAlignment = EVerticalAlignment.Center };

            programStack.Children.Add(new ImageElement("StompboxLogo")
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
            });

            HorizontalStack programHStack = new HorizontalStack() { ChildSpacing = 10, HorizontalAlignment = EHorizontalAlignment.Stretch };
            programStack.Children.Add(programHStack);

            currentProgramInterface = new EnumInterface(new List<string>(), UIColor.Black)
            {
                NoSelectionText = "-- No Preset --",
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                SelectionChangedAction = delegate (int index)
                {
                    int currentPreset = currentProgramInterface.SelectedIndex;

                    if (currentPreset != -1)
                    {
                        StompboxClient.Instance.SendCommand("LoadPreset " + currentProgramInterface.EnumValues[currentPreset]);

                        StompboxClient.Instance.UpdateProgram();
                    }
                }
            };
            programHStack.Children.Add(currentProgramInterface);

            HorizontalStack programHStack2 = new HorizontalStack() { ChildSpacing = 10, HorizontalAlignment = EHorizontalAlignment.Center }; ;
            programStack.Children.Add(programHStack2);

            programHStack2.Children.Add(new TextButton("Save")
            {
                ClickAction = delegate
                {
                    if (currentProgramInterface.SelectedIndex == -1)
                        DoSavePresetAs();
                    else
                        StompboxClient.Instance.SendCommand("SavePreset " + currentProgramInterface.SelectedIndexValue);
                }
            });
            programHStack2.Children.Add(new TextButton("Save As")
            {
                ClickAction = DoSavePresetAs
            });

            HorizontalStack programHStack3 = new HorizontalStack() { ChildSpacing = 10, HorizontalAlignment = EHorizontalAlignment.Center }; ;
            programStack.Children.Add(programHStack3);

            programHStack3.Children.Add(new TextButton("Save Settings")
            {
                ClickAction = delegate
                {
                    StompboxClient.Instance.SendCommand("SaveSettings");
                }
            });

            midiToggleButton = new TextToggleButton("Midi On", "Midi Off")
            {
                PressAction = ToggleMidi
            };

            midiToggleButton.SetPressed(true);

            programHStack3.Children.Add(midiToggleButton);

            HorizontalStack buttonStack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Center };
            programStack.Children.Add(buttonStack);

            buttonStack.Children.Add(new TextButton("Tuner")
            {
                ClickAction = delegate
                {
                    SetSelectedPlugin(new TunerInterface(StompboxClient.Instance.Tuner));
                }
            });

            buttonStack.Children.Add(new TextButton("Player")
            {
                ClickAction = delegate
                {
                    SetSelectedPlugin(new AudioFilePlayerInterface(StompboxClient.Instance.AudioPlayer));
                }
            });

            AudioFileRecorderInterface.Init();  // Starts record timer

            buttonStack.Children.Add(new TextButton("Recorder")
            {
                ClickAction = delegate
                {
                    SetSelectedPlugin(new AudioFileRecorderInterface(StompboxClient.Instance.AudioRecorder));
                }
            });

            int plugHeight = 170;

            ampStack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Center, DesiredHeight = plugHeight };
            vStack.Children.Add(ampStack);

            vStack.Children.Add(inputChainDisplay = new PluginChainDisplay("Input", miniMode: true)
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                DesiredHeight = plugHeight,
                PluginClickAction = PluginClicked
            });

            vStack.Children.Add(fxLoopDisplay = new PluginChainDisplay("FxLoop", miniMode: true)
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                DesiredHeight = plugHeight,
                PluginClickAction = PluginClicked
            });

            vStack.Children.Add(outputChainDisplay = new PluginChainDisplay("Output", miniMode: true)
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                DesiredHeight = plugHeight,
                PluginClickAction = PluginClicked
            });

            selectedPluginDock = new Dock();
            vStack.Children.Add(selectedPluginDock);

            selectedPluginWrapper = new UIElementWrapper
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                DesiredHeight = 600
            };
            selectedPluginDock.Children.Add(selectedPluginWrapper);

            if (StompboxClient.Instance.InClientMode)
            {
                dspLoadText = new StringBuilderTextBlock("---") { HorizontalAlignment = EHorizontalAlignment.Left, VerticalAlignment = EVerticalAlignment.Bottom };

                Children.Add(dspLoadText);
            }

            Children.Add(new TextButton("PB")
            {
                HorizontalAlignment = EHorizontalAlignment.Right,
                VerticalAlignment = EVerticalAlignment.Bottom,
                ClickAction = delegate
                {
                    StompboxGame.Instance.SetInterfaceType(EStompboxInterfaceType.Pedalboard);
                    StompboxClient.Instance.NeedUIReload = true;
                }
            });
        }

        void PluginClicked(PluginChainDisplay chainDisplay, PluginInterfaceBase plugin)
        {
            SetSelectedPlugin(plugin as MiniPluginInterface, chainDisplay);
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            if (StompboxClient.Instance.NeedUIReload)
            {
                UpdateUI();

                StompboxClient.Instance.NeedUIReload = false;
            }

            if (StompboxClient.Instance.InClientMode)
            {
                UpdateDSPLoad(StompboxClient.Instance.MaxDSPLoad, StompboxClient.Instance.MinDSPLoad);
            }
        }

        void ToggleMidi()
        {
            StompboxClient.Instance.SendCommand("MidiLock" + (midiToggleButton.IsPressed ? "On" : "Off"));
        }

        void DoSavePresetAs()
        {
            Task.Run(GetPresetName);
        }

        async Task GetPresetName()
        {
            string presetName = await Layout.Current.GetKeyboardInputAsync("Preset Name", "New Preset Name");

            if (!string.IsNullOrEmpty(presetName))
            {
                StompboxClient.Instance.SendCommand("SavePreset " + presetName);
                StompboxClient.Instance.SendCommand("List Presets");
                StompboxClient.Instance.UpdateProgram();
            }
        }

        public void UpdateUI()
        {
            StompboxGame.Instance.Scale = (Layout.Current as MonoGameLayout).UnscaledBounds.Height / 1920.0f;

            ampStack.Children.Clear();

            AddAmpPlugin(StompboxClient.Instance.Amp, "Amp");
            AddAmpPlugin(StompboxClient.Instance.Tonestack, "Tonestack");
            AddAmpPlugin(StompboxClient.Instance.Cabinet, "Cabinet");

            topUIStack.Children.Clear();

            topUIStack.Children.Add(new GainPluginInterface(StompboxClient.Instance.InputGain));

            topUIStack.Children.Add(programStack);

            topUIStack.Children.Add(new GainPluginInterface(StompboxClient.Instance.MasterVolume));


            inputChainDisplay.SetChain(StompboxClient.Instance.InputPlugins);
            fxLoopDisplay.SetChain(StompboxClient.Instance.FxLoopPlugins);
            outputChainDisplay.SetChain(StompboxClient.Instance.OutputPlugins);

            currentProgramInterface.SetEnumValues(StompboxClient.Instance.PresetNames);
            currentProgramInterface.SetSelectedIndex(StompboxClient.Instance.CurrentPresetIndex);

            if (currentSelectedPlugin != null)
            {
                if (!StompboxClient.Instance.AllActivePlugins.Where(p => (p.ID == currentSelectedPlugin.Plugin.ID)).Any())
                {
                    SetSelectedPlugin(null);
                }
            }

            UpdateContentLayout();
        }

        void AddAmpPlugin(IAudioPlugin plugin, string slotName)
        {
            if (plugin == null)
                return;

            MiniPluginInterface miniPlug = new MiniPluginInterface(plugin);

            miniPlug.ClickAction = delegate { SetSelectedPlugin(miniPlug, slotName); };

            ampStack.Children.Add(miniPlug);
        }

        public void SetSelectedPlugin(MiniPluginInterface plugin, PluginChainDisplay chainDisplay)
        {
            if (plugin == null)
            {
                SetSelectedPlugin(null);
            }
            else
            {
                PluginInterface pluginInterface = new PluginInterface(plugin.Plugin, new UIColor(200, 200, 200), chainDisplay)
                {
                    MinWidth = 420,
                    MiniPlugin = plugin
                };

                SetSelectedPlugin(pluginInterface);
            }

            UpdateContentLayout();
        }

        public void SetSelectedPlugin(MiniPluginInterface plugin, string slotName)
        {
            if (plugin == null)
            {
                SetSelectedPlugin(null);
            }
            else
            {
                PluginInterface pluginInterface = new PluginInterface(plugin.Plugin, slotName)
                {
                    MinWidth = 420,
                    MiniPlugin = plugin
                };

                SetSelectedPlugin(pluginInterface);
            }

            UpdateContentLayout();
        }


        public void SetSelectedPlugin(PluginInterface pluginInterface)
        {
            if ((selectedPluginWrapper.Child as PluginInterface) != null)
            {
                (selectedPluginWrapper.Child as PluginInterface).Close();
            }

            if (pluginInterface is TunerInterface)
                pluginInterface.Plugin.Enabled = true;

            currentSelectedPlugin = pluginInterface;
            selectedPluginWrapper.Child = pluginInterface;

            UpdateContentLayout();
        }

        public void UpdateDSPLoad(float maxDSPLoad, float minDSPLoad)
        {
            maxDSPLoad *= 100;
            minDSPLoad *= 100;

            dspLoadText.StringBuilder.Clear();

            int maxInt = (int)maxDSPLoad;

            dspLoadText.StringBuilder.AppendNumber(maxInt);
            dspLoadText.StringBuilder.Append('.');
            dspLoadText.StringBuilder.AppendNumber((int)Math.Round((maxDSPLoad - (float)maxInt) * 10));
            dspLoadText.StringBuilder.Append("%/");

            int minInt = (int)minDSPLoad;

            dspLoadText.StringBuilder.AppendNumber(maxInt);
            dspLoadText.StringBuilder.Append('.');
            dspLoadText.StringBuilder.AppendNumber((int)Math.Round((minDSPLoad - (float)minInt) * 10));
            dspLoadText.StringBuilder.Append('%');
        }
    }
}
