using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.Threading;
using System.Threading.Tasks;
using UILayout;

namespace Stompbox
{
    public class DAWInterface : InterfaceBase
    {
        PluginChainDisplay inputChainDisplay;
        PluginChainDisplay fxLoopDisplay;
        PluginChainDisplay outputChainDisplay;

        HorizontalStack topUIStack;
        VerticalStack programStack;
        HorizontalStack ampStack;
        EnumInterface currentProgramInterface;
        TextToggleButton midiToggleButton;
        StringBuilderTextBlock dspLoadText;
        UIElementWrapper tunerWrapper;
        UIElementWrapper audioFilePlayerWrapper;

        public DAWInterface()
        {
            Padding = new LayoutPadding(5);
            BackgroundColor = new UIColor(60, 60, 60);

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
                        StompboxClient.Instance.SaveCurrentPreset();
                }
            });
            programHStack2.Children.Add(new TextButton("Save As")
            {
                ClickAction = delegate
                {
                    DoSavePresetAs();
                }
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

            row2Stack.Children.Add(inputChainDisplay = new PluginChainDisplay("Input", miniMode: false) { HorizontalAlignment = EHorizontalAlignment.Center });

            HorizontalStack row3Stack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Stretch };
            vStack.Children.Add(row3Stack);

            row3Stack.Children.Add(audioFilePlayerWrapper = new UIElementWrapper());

            HorizontalStack loopOutputStack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Center, ChildSpacing = 50 };
            row3Stack.Children.Add(loopOutputStack);

            loopOutputStack.Children.Add(fxLoopDisplay = new PluginChainDisplay("FxLoop", miniMode: false) { HorizontalAlignment = EHorizontalAlignment.Left });
            loopOutputStack.Children.Add(outputChainDisplay = new PluginChainDisplay("Output", miniMode: false) { HorizontalAlignment = EHorizontalAlignment.Left });

            if (StompboxClient.Instance.InClientMode)
            {
                dspLoadText = new StringBuilderTextBlock("---") { HorizontalAlignment = EHorizontalAlignment.Left, VerticalAlignment = EVerticalAlignment.Bottom };
                Children.Add(dspLoadText);

                Children.Add(new ExtraOptionsButton(EStompboxInterfaceType.DAW)
                {
                    HorizontalAlignment = EHorizontalAlignment.Right,
                    VerticalAlignment = EVerticalAlignment.Bottom,
                });
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
                UpdateDSPLoad(StompboxClient.Instance.MaxDSPLoad, StompboxClient.Instance.MinDSPLoad);
            }
        }

        void ToggleMidi()
        {
            StompboxClient.Instance.SendCommand("MidiLock" + (midiToggleButton.IsPressed ? "On" : "Off"));
        }

        public void UpdateUI()
        {
            StompboxGame.Instance.Scale = (Layout.Current as MonoGameLayout).UnscaledBounds.Height / 1600.0f;

            ampStack.Children.Clear();

            AddAmpPlugin(StompboxClient.Instance.Amp, "Amp");
            AddAmpPlugin(StompboxClient.Instance.Tonestack, "Tonestack");
            AddAmpPlugin(StompboxClient.Instance.Cabinet, "Cabinet");

            topUIStack.Children.Clear();

            topUIStack.Children.Add(programStack);

            topUIStack.Children.Add(new GainPluginInterface(StompboxClient.Instance.InputGain));

            topUIStack.Children.Add(ampStack);

            topUIStack.Children.Add(new GainPluginInterface(StompboxClient.Instance.MasterVolume));

            tunerWrapper.Child = new TunerInterface(StompboxClient.Instance.Tuner);
            audioFilePlayerWrapper.Child = new AudioFilePlayerInterface(StompboxClient.Instance.AudioPlayer);

            inputChainDisplay.SetChain(StompboxClient.Instance.InputPlugins);
            fxLoopDisplay.SetChain(StompboxClient.Instance.FxLoopPlugins);
            outputChainDisplay.SetChain(StompboxClient.Instance.OutputPlugins);

            currentProgramInterface.SetEnumValues(StompboxClient.Instance.PresetNames);
            currentProgramInterface.SetSelectedIndex(StompboxClient.Instance.CurrentPresetIndex);

            UpdateContentLayout();
        }

        void AddAmpPlugin(IAudioPlugin plugin, string slotName)
        {
            if (plugin == null)
                return;

            ampStack.Children.Add(new PluginInterface(plugin, slotName) { VerticalAlignment = EVerticalAlignment.Stretch });
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
