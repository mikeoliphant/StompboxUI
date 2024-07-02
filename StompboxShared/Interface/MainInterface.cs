using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UILayout;

namespace Stompbox
{
    public class MainInterface : Dock
    {
        public static MainInterface Instance { get; private set; }

        PluginChainDisplay inputChainDisplay;
        PluginChainDisplay fxLoopDisplay;
        PluginChainDisplay outputChainDisplay;

        Dock selectedPluginDock;
        UIElementWrapper selectedPluginWrapper;

        HorizontalStack topUIStack;
        VerticalStack programStack;
        HorizontalStack ampStack;
        EnumInterface currentProgramInterface;
        TextToggleButton midiToggleButton;
        StringBuilderTextBlock dspLoadText;
        UIElementWrapper tunerWrapper;
        UIElementWrapper audioFilePlayerWrapper;

        bool clientConnected = true;

        public MainInterface()
        {
            Instance = this;

            Padding = new LayoutPadding(5);
            BackgroundColor = new UIColor(60, 60, 60);

            if (StompboxGame.DAWMode)
            {
                VerticalStack vStack = new VerticalStack
                {
                    ChildSpacing = 10,
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
                    VerticalAlignment = EVerticalAlignment.Center
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

                        StompboxClient.Instance.CurrentPresetIndex = currentPreset;

                        //if (currentPreset != -1)
                        //{
                        //    StompboxClient.Instance.SendCommand("LoadPreset " + currentProgramInterface.EnumValues[currentPreset]);

                        //    StompboxClient.Instance.UpdateProgram();
                        //}
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
                    ClickAction = delegate
                    {
                        DoSavePresetAs();
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


                HorizontalStack buttonStack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Center };
                programStack.Children.Add(buttonStack);

                midiToggleButton = new TextToggleButton("Midi On", "Midi Off")
                {
                    PressAction = ToggleMidi
                };

                midiToggleButton.SetPressed(true);

                buttonStack.Children.Add(midiToggleButton);


                ampStack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Center }; //, BackgroundColor = UIColor.FromHex("#b5893c") };

                HorizontalStack row2Stack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Stretch };
                vStack.Children.Add(row2Stack);

                row2Stack.Children.Add(tunerWrapper = new UIElementWrapper());

                row2Stack.Children.Add(inputChainDisplay = new PluginChainDisplay("Input") { HorizontalAlignment = EHorizontalAlignment.Center });

                HorizontalStack row3Stack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Stretch };
                vStack.Children.Add(row3Stack);

                row3Stack.Children.Add(audioFilePlayerWrapper = new UIElementWrapper());

                HorizontalStack loopOutputStack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Center, ChildSpacing = 50 };
                row3Stack.Children.Add(loopOutputStack);

                loopOutputStack.Children.Add(fxLoopDisplay = new PluginChainDisplay("FxLoop") { HorizontalAlignment = EHorizontalAlignment.Left });
                loopOutputStack.Children.Add(outputChainDisplay = new PluginChainDisplay("Output") { HorizontalAlignment = EHorizontalAlignment.Left });

                if (StompboxClient.Instance.InClientMode)
                {
                    dspLoadText = new StringBuilderTextBlock("---") { HorizontalAlignment = EHorizontalAlignment.Left, VerticalAlignment = EVerticalAlignment.Bottom };
                    Children.Add(dspLoadText);
                }
            }
            else
            {
                VerticalStack vStack = new VerticalStack
                {
                    ChildSpacing = 5,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch
                };
                Children.Add(vStack);

                topUIStack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Stretch};
                vStack.Children.Add(topUIStack);

                programStack = new VerticalStack { HorizontalAlignment = EHorizontalAlignment.Stretch, VerticalAlignment = EVerticalAlignment.Center };

                programStack.Children.Add(new ImageElement("StompboxLogo")
                {
                    HorizontalAlignment = EHorizontalAlignment.Center,
                    VerticalAlignment = EVerticalAlignment.Center
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

                vStack.Children.Add(inputChainDisplay = new PluginChainDisplay("Input") { HorizontalAlignment = EHorizontalAlignment.Center, DesiredHeight = plugHeight });
                vStack.Children.Add(fxLoopDisplay = new PluginChainDisplay("FxLoop") { HorizontalAlignment = EHorizontalAlignment.Center, DesiredHeight = plugHeight });
                vStack.Children.Add(outputChainDisplay = new PluginChainDisplay("Output") { HorizontalAlignment = EHorizontalAlignment.Center, DesiredHeight = plugHeight });

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
            }
        }

        public void Connect()
        {
            clientConnected = false;

            string serverName = "raspberrypi";

            //PixSaveState.TryGetString("ServerName", ref serverName);

            StompboxClient.Instance.Connect(serverName, 24639, ConnectCallback);
        }

        void ConnectCallback(bool connected)
        {
            if (!connected)
                Task.Run(GetServerName);
            else
                clientConnected = true;
        }

        async Task GetServerName()
        {
            try
            {
                // Wait to make sure UI is active
                Thread.Sleep(1000);

                string serverName = await Layout.Current.GetKeyboardInputAsync("Enter server:", "raspberrypi");

                //PixSaveState.SetString("ServerName", serverName);
                //PixSaveState.Save();

                StompboxClient.Instance.Connect(serverName, 24639, ConnectCallback);
            }
            catch (Exception ex)
            {

            }
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
                if (clientConnected && !StompboxClient.Instance.Connected)
                {
                    Connect();
                }

                UpdateDSPLoad(StompboxClient.Instance.MaxDSPLoad, StompboxClient.Instance.MinDSPLoad);
            }

            UpdateContentLayout();
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
            //PixGame.Debug("Update MainInterface UI");

            ampStack.Children.Clear();

            AddAmpPlugin(StompboxClient.Instance.Amp, "Amp");
            AddAmpPlugin(StompboxClient.Instance.Tonestack, "Tonestack");
            AddAmpPlugin(StompboxClient.Instance.CabConvolver, "Cabinet");

            if (StompboxGame.DAWMode)
            {
                topUIStack.Children.Clear();

                topUIStack.Children.Add(programStack);

                topUIStack.Children.Add(new GainPluginInterface(StompboxClient.Instance.InputGain));

                topUIStack.Children.Add(ampStack);

                topUIStack.Children.Add(new GainPluginInterface(StompboxClient.Instance.MasterVolume));

                tunerWrapper.Child = new TunerInterface(StompboxClient.Instance.PluginFactory.CreatePlugin("Tuner"));
                audioFilePlayerWrapper.Child = new AudioFilePlayerInterface(StompboxClient.Instance.PluginFactory.CreatePlugin("AudioFilePlayer"));
            }
            else
            {
                topUIStack.Children.Clear();

                topUIStack.Children.Add(new GainPluginInterface(StompboxClient.Instance.InputGain));

                topUIStack.Children.Add(programStack);

                topUIStack.Children.Add(new GainPluginInterface(StompboxClient.Instance.MasterVolume));

            }

            inputChainDisplay.SetChain(StompboxClient.Instance.InputPlugins);
            fxLoopDisplay.SetChain(StompboxClient.Instance.FxLoopPlugins);
            outputChainDisplay.SetChain(StompboxClient.Instance.OutputPlugins);

            currentProgramInterface.SetEnumValues(StompboxClient.Instance.PresetNames);
            currentProgramInterface.SetSelectedIndex(StompboxClient.Instance.CurrentPresetIndex);
        }

        void AddAmpPlugin(IAudioPlugin plugin, string slotName)
        {
            if (plugin == null)
                return;

            if (StompboxGame.DAWMode)
            {
                ampStack.Children.Add(new PluginInterface(plugin, slotName) { VerticalAlignment = EVerticalAlignment.Stretch });
            }
            else
            {
                MiniPluginInterface miniPlug = new MiniPluginInterface(plugin);

                miniPlug.ClickAction = delegate { MainInterface.Instance.SetSelectedPlugin(miniPlug, slotName); };

                ampStack.Children.Add(miniPlug);
            }
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
