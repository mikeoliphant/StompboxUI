using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UILayout;

namespace Stompbox
{
    public class MobileInterface : InterfaceBase
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
            programHStack2.Children.Add(new TextButton("Delete")
            {
                ClickAction = delegate
                {
                    if (currentProgramInterface.SelectedIndex != -1)
                    {
                        Layout.Current.ShowConfirmationPopup("Are you sure you want to\ndelete this preset?",
                            delegate
                            {
                                StompboxClient.Instance.DeleteCurrentPreset();
                            });
                    }
                }
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
                    SetSelectedPlugin(new TunerInterface(StompboxClient.Instance.PluginFactory.CreatePlugin("Tuner")));
                }
            });

            buttonStack.Children.Add(new TextButton("Player")
            {
                ClickAction = delegate
                {
                    SetSelectedPlugin(new AudioFilePlayerInterface(StompboxClient.Instance.PluginFactory.CreatePlugin("AudioFilePlayer")));
                }
            });

            buttonStack.Children.Add(new TextButton("Recorder")
            {
                ClickAction = delegate
                {
                    SetSelectedPlugin(new AudioFileRecorderInterface(StompboxClient.Instance.PluginFactory.CreatePlugin("AudioRecorder")));
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

            //Children.Add(new TextButton("PB")
            //{
            //    HorizontalAlignment = EHorizontalAlignment.Right,
            //    VerticalAlignment = EVerticalAlignment.Bottom,
            //    ClickAction = delegate
            //    {
            //        StompboxLayout.Instance.SetInterfaceType(EStompboxInterfaceType.Pedalboard);
            //        StompboxClient.Instance.NeedUIReload = true;
            //    }
            //});

            Children.Add(new ExtraOptionsButton(EStompboxInterfaceType.Mobile)
            {
                HorizontalAlignment = EHorizontalAlignment.Right,
                VerticalAlignment = EVerticalAlignment.Bottom,
            });
        }

        void PluginClicked(PluginChainDisplay chainDisplay, PluginInterfaceBase plugin)
        {
            SetSelectedPlugin(plugin as MiniPluginInterface, chainDisplay);
        }

        void ToggleMidi()
        {
            StompboxClient.Instance.SendCommand("MidiLock" + (midiToggleButton.IsPressed ? "On" : "Off"));
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            float desiredScale = (Layout.Current as MonoGameLayout).UnscaledBounds.Height / 1920.0f;

            if (StompboxLayout.Instance.Scale != desiredScale)
                StompboxLayout.Instance.Scale = desiredScale;


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

        public void UpdateUI()
        {
            ampStack.Children.Clear();

            AddAmpPlugin("Amp");
            AddAmpPlugin("Tonestack");
            AddAmpPlugin("Cabinet");

            topUIStack.Children.Clear();

            topUIStack.Children.Add(new GainPluginInterface(StompboxClient.Instance.PluginFactory.CreatePlugin("Input")));

            topUIStack.Children.Add(programStack);

            topUIStack.Children.Add(new GainPluginInterface(StompboxClient.Instance.PluginFactory.CreatePlugin("Master")));


            inputChainDisplay.SetChain(StompboxClient.Instance.GetChain("Input"));
            fxLoopDisplay.SetChain(StompboxClient.Instance.GetChain("FxLoop"));
            outputChainDisplay.SetChain(StompboxClient.Instance.GetChain("Output"));

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

        void AddAmpPlugin(string slotName)
        {
            IAudioPlugin plugin = StompboxClient.Instance.PluginFactory.CreatePlugin(StompboxClient.Instance.GetSlotPlugin(slotName));

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
