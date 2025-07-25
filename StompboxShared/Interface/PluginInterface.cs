﻿using System;
using System.Collections;
using System.Collections.Generic;
using UILayout;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using UILayout;

namespace Stompbox
{
    public class PluginChainDisplay : Dock
    {
        public string Name { get; private set; }
        public Action<PluginChainDisplay, PluginInterfaceBase> PluginClickAction { get; set; }

        HorizontalStack pluginStack;

        List<IAudioPlugin> plugins = new List<IAudioPlugin>();
        TextButton addPluginButton;
        bool miniMode;

        public PluginChainDisplay(string name, bool miniMode)
        {
            this.Name = name;
            this.miniMode = miniMode;

            VerticalAlignment = EVerticalAlignment.Top;

            HorizontalStack mainHStack = new HorizontalStack() { VerticalAlignment = EVerticalAlignment.Stretch };
            Children.Add(mainHStack);

            if (!miniMode)
            {
                UIImage image = Layout.Current.GetImage(name + "Chain");

                if (image != null)
                    mainHStack.Children.Add(new ImageElement(image) { VerticalAlignment = EVerticalAlignment.Center });
            }

            pluginStack = new HorizontalStack() { ChildSpacing = 0, VerticalAlignment = EVerticalAlignment.Stretch };
            mainHStack.Children.Add(pluginStack);

            ListUIDragDropHandler dragDropHandler = new ListUIDragDropHandler();
            dragDropHandler.ListElement = pluginStack;
            dragDropHandler.InternalOnly = false;
            dragDropHandler.DragType = typeof(PluginInterfaceBase);
            pluginStack.DragDropHandler = dragDropHandler;

            dragDropHandler.DragCompleteAction = delegate (object dropObject)
            {
                PluginInterface dropInterface = dropObject as PluginInterface;

                if (dropInterface.ChainDisplay != this)
                {
                    dropInterface.ChainDisplay.UpdateChain();

                    dropInterface.ChainDisplay = this;
                }

                UpdateChain();
            };

            mainHStack.Children.Add(addPluginButton = new TextButton("Add\n" + name)
            {
                DesiredHeight = PluginInterface.DefaultHeight,
                VerticalAlignment = EVerticalAlignment.Stretch,
                ClickAction = AddPlugin
            });
        }

        public void UpdateChain()
        {
            plugins.Clear();

            foreach (PluginInterfaceBase pluginInterface in pluginStack.Children)
            {
                plugins.Add(pluginInterface.Plugin);
            }

            StompboxClient.Instance.UpdateChain(this.Name, this.plugins);
        }

        public void SetChain(List<IAudioPlugin> plugins)
        {
            this.plugins = plugins;

            pluginStack.Children.Clear();

            foreach (IAudioPlugin plugin in plugins)
            {
                AddPluginToChain(plugin);
            }
        }

        void AddPluginToChain(IAudioPlugin newPlugin)
        {
            PluginInterfaceBase pluginInterface;

            if (!miniMode)
            {
                pluginInterface = new PluginInterface(newPlugin, this) { VerticalAlignment = EVerticalAlignment.Stretch };
            }
            else
            {
                pluginInterface = new MiniPluginInterface(newPlugin);
            }

            pluginInterface.ClickAction = delegate
            {
                PluginClicked(pluginInterface);
            };

            pluginStack.Children.Add(pluginInterface);
        }

        void PluginClicked(PluginInterfaceBase pluginInterface)
        {
            if (PluginClickAction != null)
            {
                PluginClickAction(this, pluginInterface);
            }
        }

        public void RemovePlugin(IAudioPlugin plugin)
        {
            Layout.Current.ShowConfirmationPopup("Are you sure you want to\ndelete this plugin?",
                delegate
                {
                    PluginInterfaceBase toDelete = null;

                    foreach (PluginInterfaceBase pluginInterface in pluginStack.Children)
                    {
                        if (pluginInterface.Plugin == plugin)
                        {
                            toDelete = pluginInterface;
                            break;
                        }
                    }

                    if (toDelete != null)
                    {
                        pluginStack.Children.Remove(toDelete);

                        if (PluginClickAction != null)
                            PluginClickAction(this, null);

                        UpdateContentLayout();

                        UpdateChain();

                        StompboxClient.Instance.PluginFactory.ReleasePlugin(toDelete.Plugin);
                    }
                });
        }

        void AddPlugin()
        {
            Layout.Current.ShowPopup(new PluginFlipList(StompboxClient.Instance.GetAllUserPluginDefinitions())
            {
                DesiredWidth = Layout.Current.Bounds.Width,
                DesiredHeight = 260,
                SelectAction = delegate (string pluginName)
                {
                    IAudioPlugin newPlugin = StompboxClient.Instance.PluginFactory.CreateNewPlugin(pluginName);

                    newPlugin.Enabled = true;

                    AddPluginToChain(newPlugin);

                    UpdateChain();
                    UpdateContentLayout();
                }
            });
        }
    }

    public class PluginFlipList : FlipList, IPopup
    {
        public Action<string> SelectAction { get; set; }
        public Action CloseAction { get; set; }

        public PluginFlipList(IEnumerable<IAudioPlugin> plugins)
            : base(230)
        {
            HorizontalAlignment = EHorizontalAlignment.Center;
            VerticalAlignment = EVerticalAlignment.Center;

            BackgroundColor = new UIColor(120, 120, 120);
            Padding = new LayoutPadding(0, 50);

            foreach (IAudioPlugin plugin in plugins)
            {
                Children.Add(new MiniPluginInterface(plugin)
                {
                    MinWidth = 220,

                    ClickAction = delegate
                    {
                        if (SelectAction != null)
                            SelectAction(plugin.Name);

                        CloseAction();
                    }
                });
            }
        }

        public override bool HandleTouch(in Touch touch)
        {
            if (!ContentBounds.Contains(touch.Position))
            {
                CloseAction();

                return true;
            }

            return base.HandleTouch(touch);
        }

        public void Opened()
        {
        }
    }

    public class PluginInterfaceBase : UIElementWrapper
    {
        public IAudioPlugin Plugin { get; private set; }
        public Action ClickAction { get; set; }
        public bool ShowOutputParameters { get; set; } = true;

        protected UIColor backgroundColor;
        protected UIColor foregroundColor;

        public PluginInterfaceBase()
        {

        }

        public PluginInterfaceBase(IAudioPlugin plugin)
        {
            SetPlugin(plugin);
        }

        public override string ToString()
        {
            return Plugin.ToString();
        }

        public virtual void SetPlugin(IAudioPlugin plugin)
        {
            this.Plugin = plugin;

            backgroundColor = UIColor.FromHex(plugin.BackgroundColor);

            if (backgroundColor == UIColor.Transparent)
                backgroundColor = new UIColor(200, 200, 200);

            foregroundColor = UIColor.FromHex(plugin.ForegroundColor);

            if (foregroundColor == UIColor.Transparent)
                foregroundColor = new UIColor(30, 30, 30);
        }

        protected virtual void AddControls(Dock dock)
        {
        }

        protected UIElement CreateControl(PluginParameter parameter)
        {
            VerticalStack controlVStack = new VerticalStack()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
            };

            controlVStack.Children.Add(new TextBlock(parameter.Name)
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                TextColor = foregroundColor,
                TextFont = Layout.Current.GetFont("SmallFont"),
            });

            Dock controlDock = new Dock() { HorizontalAlignment = EHorizontalAlignment.Stretch, VerticalAlignment = EVerticalAlignment.Center };
            controlVStack.Children.Add(controlDock);

            if (parameter.ParameterType == EParameterType.Bool)
            {
                PowerButton paramButton = new PowerButton()
                {
                    HorizontalAlignment = EHorizontalAlignment.Center,
                    VerticalAlignment = EVerticalAlignment.Center,
                };

                paramButton.PressAction = delegate
                {
                    parameter.Value = paramButton.IsPressed ? 1 : 0;
                };

                paramButton.SetPressed(parameter.Value > 0);

                parameter.SetValue = delegate (float val)
                {
                    paramButton.SetPressed(val > 0);
                };

                controlVStack.Children.Add(new UIElementWrapper()
                {
                    Child = paramButton,
                    HorizontalAlignment = EHorizontalAlignment.Stretch,
                    VerticalAlignment = EVerticalAlignment.Stretch,
                });
            }
            else if ((parameter.ParameterType == EParameterType.VSlider) || (parameter.ParameterType == EParameterType.Knob))
            {
                float strWidthMax;
                float strHeightMax;

                Layout.Current.GetFont("SmallFont").MeasureString(String.Format(parameter.ValueFormat, parameter.MaxValue), out strWidthMax, out strHeightMax);

                float strWidthMin;
                float strHeightMin;

                Layout.Current.GetFont("SmallFont").MeasureString(String.Format(parameter.ValueFormat, parameter.MinValue), out strWidthMin, out strHeightMin);

                ParameterValueDisplay valueDisplay = new ParameterValueDisplay()
                {
                    HorizontalAlignment = EHorizontalAlignment.Absolute,
                    VerticalAlignment = EVerticalAlignment.Absolute,
                    Margin = new LayoutPadding(-Math.Max(strWidthMin, strWidthMax), -Math.Max(strHeightMin, strHeightMax)),
                    ValueFormat = parameter.ValueFormat
                };

                if (parameter.ParameterType == EParameterType.VSlider)
                {
                    controlVStack.DesiredWidth = 80;
                    controlDock.BackgroundColor = new UIColor(50, 50, 50);

                    VerticalSlider slider = new VerticalSlider("VerticalSlider")
                    {
                        HorizontalAlignment = EHorizontalAlignment.Stretch,
                        InvertLevel = true,
                        DesiredHeight = 150,
                        DesiredWidth = 40,
                        ChangeAction = delegate (float value)
                        {
                            parameter.Value = parameter.MinValue + ((parameter.MaxValue - parameter.MinValue) * value);

                            valueDisplay.SetValue(parameter.Value);

                            UpdateContentLayout();
                        }
                    };

                    controlDock.Children.Add(slider);

                    slider.SetLevel((float)((parameter.Value - parameter.MinValue) / (parameter.MaxValue - parameter.MinValue)));
                }
                else
                {
                    controlVStack.DesiredWidth = 130;

                    ParameterDial dial = new ParameterDial()
                    {
                        MinValue = 0,
                        MaxValue = 1,
                        DefaultValue = parameter.GetNormalizedValue(parameter.DefaultValue)
                    };

                    controlDock.Children.Add(dial);

                    dial.SetDialColor(foregroundColor);

                    dial.SetPointerColor(((foregroundColor.R + foregroundColor.G + foregroundColor.B) / 3) > 128 ? UIColor.Black : UIColor.White);

                    dial.SetValue(parameter.NormalizedValue);

                    dial.ValueChangedAction = delegate (float val)
                    {
                        parameter.NormalizedValue = val;

                        valueDisplay.SetValue(parameter.Value);
                    };

                    //dial.HoldAction = delegate
                    //{
                    //    MainInterface.Instance.ProtocolClient.SendCommand("MapController Expression " + Plugin.ID + " " + parameter.Name);
                    //};

                    parameter.SetValue = delegate (float value)
                    {
                        dial.SetValue(parameter.GetNormalizedValue(value));
                    };

                    if (parameter.CanSyncToHostBPM)
                    {
                        parameter.UpdateBPMSync = delegate
                        {
                            if ((parameter.HostBPMSyncNumerator != 0) && (parameter.HostBPMSyncDenominator != 0))
                            {
                                StompboxClient.Instance.SendCommand("SetParam " + Plugin.ID + " " + parameter.Name + " " + parameter.HostBPMSyncNumerator + "/" + parameter.HostBPMSyncDenominator);

                                parameter.Value = ((60.0f / StompboxClient.Instance.BPM) * ((float)parameter.HostBPMSyncNumerator / (float)parameter.HostBPMSyncDenominator)) * 1000;
                                dial.SetValue(parameter.Value);
                            }
                            else
                            {
                                StompboxClient.Instance.SendCommand("SetParam " + Plugin.ID + " " + parameter.Name + " " + parameter.DefaultValue);

                                dial.SetValue(parameter.GetNormalizedValue(parameter.DefaultValue));
                            }
                        };
                    }
                }

                controlDock.Children.Add(valueDisplay);
            }
            else if (parameter.ParameterType == EParameterType.Power)
            {
                PowerLevel levelDisplay = new()
                {
                    HorizontalAlignment = EHorizontalAlignment.Center,
                    VerticalAlignment = EVerticalAlignment.Center
                };

                parameter.SetValue = delegate (float value)
                {
                    levelDisplay.SetLevel(parameter.GetNormalizedValue(value));
                };

                controlDock.Children.Add(levelDisplay);
            }

            return controlVStack;
        }

        protected UIElement CreateEnumControl(PluginParameter parameter)
        {
            EnumInterface enumInterface = new EnumInterface(parameter.EnumValues, foregroundColor) { HorizontalAlignment = EHorizontalAlignment.Stretch, VerticalAlignment = EVerticalAlignment.Top, Padding = new LayoutPadding(0, 5) };

            enumInterface.SelectionChangedAction = delegate (int index)
            {
                parameter.Value = index;

                //StompboxClient.Instance.SendCommand("SetParam " + Plugin.ID + " " + parameter.Name + " " + parameter.EnumValues[index]);

                UpdateContentLayout();
            };

            enumInterface.SetSelectedIndex((int)parameter.Value);

            parameter.SetValue = delegate (float val)
            {

            };

            return enumInterface;
        }

        protected UIElement CreateFileControl(PluginParameter parameter)
        {
            bool showOpenFolder = !StompboxClient.Instance.InClientMode;

            FileInterface fileInterface = new FileInterface(showOpenFolder ? parameter.FilePath : null, parameter.EnumValues, foregroundColor)
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Top,
                Padding = new LayoutPadding(0, 5)
            };

            fileInterface.SelectionChangedAction = delegate (int index)
            {
                parameter.Value = index;

                UpdateContentLayout();
            };

            fileInterface.SetSelectedIndex((int)parameter.Value);

            parameter.SetValue = delegate (float val)
            {

            };

            return fileInterface;
        }

        protected UIElement CreateIntControl(PluginParameter parameter)
        {
            HorizontalStack intStack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Stretch, ChildSpacing = 5 };

            //intStack.Children.Add(new TextBlock(parameter.Name)
            //{
            //    Font = PixGame.Instance.GetFont("SmallFont"),
            //    TextColor = foregroundColor,
            //    VerticalAlignment = EVerticalAlignment.Center
            //});

            List<string> intValues = new List<string>();

            for (int i = (int)parameter.MinValue; i <= (int)parameter.MaxValue; i++)
            {
                intValues.Add(i.ToString());
            }

            EnumInterface enumInterface = new EnumInterface(intValues, foregroundColor) { HorizontalAlignment = EHorizontalAlignment.Stretch, Padding = new LayoutPadding(0, 5) };

            enumInterface.SelectionChangedAction = delegate (int index)
            {
                parameter.Value = parameter.MinValue + index;

                //StompboxClient.Instance.SendCommand("SetParam " + Plugin.ID + " " + parameter.Name + " " + parameter.Value);

                UpdateContentLayout();
            };

            enumInterface.SetSelectedIndex((int)parameter.Value - 1);

            parameter.SetValue = delegate (float val)
            {

            };

            intStack.Children.Add(enumInterface);

            return intStack;
        }
    }

    public class MiniPluginInterface : PluginInterfaceBase
    {
        public float MinWidth { get; set; } = 0;

        public MiniPluginInterface(IAudioPlugin plugin)
            : base(plugin)
        {
            HorizontalAlignment = EHorizontalAlignment.Center;
            VerticalAlignment = EVerticalAlignment.Stretch;

            SetPlugin(plugin);
        }

        public override void SetPlugin(IAudioPlugin plugin)
        {
            base.SetPlugin(plugin);

            Dock controlDock = new Dock();

            Child = new NinePatchWrapper(Layout.Current.GetImage("PluginBackground")) { Child = controlDock, Color = backgroundColor, HorizontalAlignment = EHorizontalAlignment.Stretch, VerticalAlignment = EVerticalAlignment.Stretch };

            AddControls(controlDock);
        }

        protected override void GetContentSize(out float width, out float height)
        {
            base.GetContentSize(out width, out height);

            if (width < MinWidth)
                width = MinWidth;
        }

        protected override void AddControls(Dock dock)
        {
            VerticalStack vStack = new VerticalStack { HorizontalAlignment = EHorizontalAlignment.Stretch, VerticalAlignment = EVerticalAlignment.Stretch };
            dock.Children.Add(vStack);

            vStack.Children.Add(new TextBlock(Plugin.Name)
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                TextColor = foregroundColor,
                TextFont = Layout.Current.GetFont("SmallFont")
            });

            HorizontalStack controlStack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Center };
            vStack.Children.Add(controlStack);

            foreach (PluginParameter parameter in Plugin.Parameters)
            {
                if (!ShowOutputParameters && parameter.IsOutput)
                    continue;

                if (!parameter.IsAdvanced && !(parameter.ParameterType == EParameterType.Enum) && !(parameter.ParameterType == EParameterType.File) && !(parameter.ParameterType == EParameterType.Int))
                {
                    if (parameter.ParameterType == EParameterType.VSlider)
                    {
                        controlStack.Children.Add(new UIElementWrapper
                        {
                            // BackgroundColor = new UIColor(50, 50, 50),
                            DesiredWidth = 15,
                            Child =
                            new TextBlock("|")
                            {
                                TextColor = foregroundColor,
                                HorizontalAlignment = EHorizontalAlignment.Center
                            }
                        });
                    }
                    else
                    {
                        controlStack.Children.Add(new UIElementWrapper
                        {
                            DesiredWidth = 30,
                            Child =
                            new TextBlock("o")
                            {
                                TextColor = foregroundColor,
                                HorizontalAlignment = EHorizontalAlignment.Center
                            }
                        });
                    }
                }
            }
        }

        public override bool HandleTouch(in Touch touch)
        {
            if (touch.TouchState == ETouchState.Released)
            {
                if (ClickAction != null)
                    ClickAction();

                return true;
            }

            return base.HandleTouch(touch);
        }
    }

    public class PluginInterface : PluginInterfaceBase
    {
        public static float DefaultHeight = 550;

        public UIColor DefaultBackgroundColor { get; private set; }
        public float MinWidth { get; set; } = 320;
        public MiniPluginInterface MiniPlugin { get; set; }
        public PluginChainDisplay ChainDisplay { get; set; }

        string slotName;
        VerticalStack controlStack;
        Menu menu = new Menu();
        List<MenuItem> menuItems = new List<MenuItem>();
        protected bool showOptionsMenu = true;
        protected bool showAdvancedControls = false;

        public PluginInterface()
        {
        }

        public PluginInterface(IAudioPlugin plugin)
            : this(plugin, new UIColor(200, 200, 200), null)
        {
        }

        public PluginInterface(IAudioPlugin plugin, string slotName)
            : this(plugin)
        {
            this.slotName = slotName;
        }

        public PluginInterface(IAudioPlugin plugin, PluginChainDisplay chainDisplay)
            : this(plugin, new UIColor(200, 200, 200), chainDisplay)
        {
        }

        public PluginInterface(IAudioPlugin plugin, UIColor defaultBackgroundColor, PluginChainDisplay chainDisplay)
        {
            this.ChainDisplay = chainDisplay;

            SetPlugin(plugin, defaultBackgroundColor);
        }

        public override void SetPlugin(IAudioPlugin plugin)
        {
            SetPlugin(plugin, new UIColor(200, 200, 200));
        }

        void SetPlugin(IAudioPlugin plugin, UIColor defaultBackgroundColor)
        {
            base.SetPlugin(plugin);

            this.DefaultBackgroundColor = defaultBackgroundColor;

            DesiredHeight = DefaultHeight;

            Dock controlDock = new Dock();

            Child = new NinePatchWrapper(Layout.Current.GetImage("PluginBackground")) { Child = controlDock, Color = backgroundColor, HorizontalAlignment = EHorizontalAlignment.Stretch, VerticalAlignment = EVerticalAlignment.Stretch };

            AddControls(controlDock);
        }

        protected void AddMenuItem(MenuItem item)
        {
            menuItems.Add(item);
        }

        protected override void AddControls(Dock dock)
        {
            if (showOptionsMenu && (ChainDisplay != null))
            {
                AddMenuItem(new ContextMenuItem
                {
                    Text = "Remove Plugin",
                    AfterCloseAction = delegate
                    {
                        ChainDisplay.RemovePlugin(this.Plugin);
                    }
                });
            }

            AddMenuItem(new ContextMenuItem
            {
                Text = "Plugin Info",
                AfterCloseAction = delegate
                {
                    string info = Plugin.Name + ": " + Plugin.Description + "\n\n\n";

                    foreach (PluginParameter param in Plugin.Parameters)
                    {
                        info += param.Name + ": " + param.Description + "\n\n";
                    }

                    Layout.Current.ShowContinuePopup(new TextBlock(info) { Margin = new LayoutPadding(20) });
                }
            });

            controlStack = new VerticalStack { HorizontalAlignment = EHorizontalAlignment.Stretch, VerticalAlignment = EVerticalAlignment.Stretch, ChildSpacing = 5 };
            dock.Children.Add(controlStack);

            UpdateControlDisplay();

            PowerButton powerButton = new PowerButton()
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Bottom,
            };

            powerButton.PressAction = delegate
            {
                PowerButtonPressed(powerButton.IsPressed);
            };

            powerButton.SetPressed(Plugin.Enabled);
            Plugin.SetEnabled = delegate (bool enabled)
            {
                powerButton.SetPressed(enabled);
            };

            dock.Children.Add(powerButton);

            if (showOptionsMenu)
            {
                ImageButton menuButton = new ImageButton("MoreButton")
                {
                    HorizontalAlignment = EHorizontalAlignment.Right,
                    VerticalAlignment = EVerticalAlignment.Bottom,
                    ImageColor = foregroundColor
                };

                //menuButton.Color = foregroundColor;

                List<MenuItem> syncItems = new List<MenuItem>();
                List<MenuItem> midiItems = new List<MenuItem>();

                midiItems.Add(new ContextMenuItem
                {
                    Text = "Clear Map",
                    AfterCloseAction = delegate
                    {
                        StompboxClient.Instance.SendCommand("ClearControllerMap");
                    }
                });

                midiItems.Add(new ContextMenuItem
                {
                    Text = "Enabled",
                    AfterCloseAction = delegate
                    {
                        Layout.Current.ShowPopup(new MidiCCMapPopup(Plugin.ID, "Enabled"), menuButton.ContentBounds.Center);
                    }
                });

                bool haveAdvancedControls = false;

                foreach (PluginParameter parameter in Plugin.Parameters)
                {
                    if (parameter.IsAdvanced)
                        haveAdvancedControls = true;

                    if (parameter.CanSyncToHostBPM)
                    {
                        List<MenuItem> paramSyncItems = new List<MenuItem>();

                        foreach (BPMSync timing in BPMSync.Timings)
                        {
                            paramSyncItems.Add(new ContextMenuItem
                            {
                                Text = timing.Name,
                                AfterCloseAction = delegate
                                {
                                    parameter.HostBPMSyncNumerator = timing.Numerator;
                                    parameter.HostBPMSyncDenominator = timing.Denomenator;

                                    if (parameter.UpdateBPMSync != null)
                                        parameter.UpdateBPMSync();
                                }
                            });
                        }

                        Menu syncMenu = new Menu(paramSyncItems);

                        syncItems.Add(new ContextMenuItem
                        {
                            Text = parameter.Name,
                            AfterCloseAction = delegate
                            {
                                Layout.Current.ShowPopup(syncMenu, menuButton.ContentBounds.Center);
                            }
                        });
                    }

                    midiItems.Add(new ContextMenuItem
                    {
                        Text = parameter.Name,
                        AfterCloseAction = delegate
                        {
                            Layout.Current.ShowPopup(new MidiCCMapPopup(Plugin.ID, parameter.Name), menuButton.ContentBounds.Center);
                        }
                    });
                }

                if (haveAdvancedControls)
                {
                    menuItems.Add(new ContextMenuItem
                    {
                        Text = "Toggle Advanced Controls",
                        AfterCloseAction = delegate
                        {
                            showAdvancedControls = !showAdvancedControls;
                            UpdateControlDisplay();
                        }
                    });
                }

                menuItems.Add(new ContextMenuItem()
                {
                    Text = "Swap Plugin",
                    AfterCloseAction = delegate
                    {
                        Layout.Current.ShowPopup(new PluginFlipList(StompboxClient.Instance.GetAllUserPluginDefinitions())
                        {
                            DesiredWidth = Layout.Current.Bounds.Width,
                            DesiredHeight = 260,
                            SelectAction = delegate (string pluginName)
                            {
                                bool enabled = this.Plugin.Enabled;

                                if (this.Plugin != null)
                                    StompboxClient.Instance.PluginFactory.ReleasePlugin(this.Plugin);

                                IAudioPlugin newPlugin = StompboxClient.Instance.PluginFactory.CreateNewPlugin(pluginName);

                                newPlugin.Enabled = enabled;

                                SetPlugin(newPlugin);

                                if (slotName != null)
                                {
                                    if (MiniPlugin != null)
                                    {
                                        MiniPlugin.SetPlugin(newPlugin);
                                    }

                                    string cmd = "SetPluginSlot " + slotName + " " + Plugin.ID;

                                    StompboxClient.Instance.SendCommand(cmd);
                                }
                                else if (ChainDisplay != null)
                                {
                                    ChainDisplay.UpdateChain();
                                }

                                UpdateContentLayout();
                            }
                        });
                    }
                });

                if (midiItems.Count > 0)
                {
                    Menu midiParamMenu = new Menu(midiItems);

                    menuItems.Add(new ContextMenuItem
                    {
                        Text = "MIDI CC Map",
                        AfterCloseAction = delegate
                        {
                            Layout.Current.ShowPopup(midiParamMenu, menuButton.ContentBounds.Center);
                        }
                    });
                }

                if (syncItems.Count > 0)
                {
                    Menu syncParamMenu = new Menu(syncItems);

                    menuItems.Add(new ContextMenuItem
                    {
                        Text = "Sync To BPM",
                        AfterCloseAction = delegate
                        {
                            Layout.Current.ShowPopup(syncParamMenu, menuButton.ContentBounds.Center);
                        }
                    });
                }

                if (menuItems.Count > 0)
                {
                    menu.SetMenuItems(menuItems);

                    menuButton.ClickAction = delegate
                    {
                        Layout.Current.ShowPopup(menu, menuButton.ContentBounds.Center);
                    };

                    dock.Children.Add(menuButton);
                }
            }
        }

        protected virtual void PowerButtonPressed(bool pressed)
        {
            Plugin.Enabled = pressed;
        }

        public void UpdateControlDisplay()
        {
            controlStack.Children.Clear();

            controlStack.Children.Add(new TextBlock(Plugin.Name) { HorizontalAlignment = EHorizontalAlignment.Center, TextColor = foregroundColor });

            HorizontalStack currentControlStack = null;

            bool haveTextParams = false;
            bool haveVerticalSliders = false;

            HorizontalStack textStack = null;

            int paramCount = 0;

            foreach (PluginParameter parameter in Plugin.Parameters)
            {
                if (!parameter.IsAdvanced || showAdvancedControls)
                {
                    if ((parameter.ParameterType == EParameterType.Enum) || (parameter.ParameterType == EParameterType.File) || (parameter.ParameterType == EParameterType.Int))
                    {
                        haveTextParams = true;
                    }
                    else if (parameter.ParameterType == EParameterType.VSlider)
                    {
                        haveVerticalSliders = true;
                    }
                    else
                    {
                        paramCount++;
                    }
                }
            }

            if (haveTextParams)
            {
                textStack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Stretch, VerticalAlignment = EVerticalAlignment.Center };
            }

            int controlsPerLine = 0;

            if (!haveTextParams && (paramCount > 2))
            {
                controlsPerLine = (int)Math.Ceiling((float)paramCount / 2.0f);
            }

            int controlsOnLine = 0;

            foreach (PluginParameter parameter in Plugin.Parameters)
            {
                if (!ShowOutputParameters && parameter.IsOutput)
                    continue;

                if (!parameter.IsAdvanced || showAdvancedControls)
                {
                    if ((parameter.ParameterType == EParameterType.Enum) || (parameter.ParameterType == EParameterType.File) || (parameter.ParameterType == EParameterType.Int))
                    {
                        if (parameter.ParameterType == EParameterType.Enum)
                        {
                            textStack.Children.Add(CreateEnumControl(parameter));
                        }
                        else if (parameter.ParameterType == EParameterType.File)
                        {
                            textStack.Children.Add(CreateFileControl(parameter));
                        }
                        else if (parameter.ParameterType == EParameterType.Int)
                        {
                            if (parameter.MinValue != parameter.MaxValue)
                            {
                                textStack.Children.Add(CreateIntControl(parameter));
                            }
                        }
                    }
                    else
                    {
                        if (currentControlStack == null)
                        {
                            currentControlStack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Center };
                            controlStack.Children.Add(currentControlStack);
                        }

                        currentControlStack.Children.Add(CreateControl(parameter));

                        controlsOnLine++;

                        if (controlsOnLine == controlsPerLine)
                        {
                            controlsOnLine = 0;
                            currentControlStack = null;
                        }
                    }
                }
            }

            if (haveTextParams)
            {
                controlStack.Children.Add(textStack);
            }

            UpdateContentLayout();
        }

        public virtual void Close()
        {
        }

        protected override void GetContentSize(out float width, out float height)
        {
            base.GetContentSize(out width, out height);

            if (width < MinWidth)
                width = MinWidth;
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            if (Plugin.Enabled)
            {
                foreach (PluginParameter param in Plugin.Parameters)
                {
                    if (param.IsOutput)
                    {
                        param.Value = param.Value;
                    }
                }
            }
        }
    }

    public class MidiCCMapPopup : NinePatchWrapper, IPopup
    {
        public Action CloseAction { get; set; }

        public MidiCCMapPopup(string pluginID, string parameterName)
            : base(Layout.Current.DefaultOutlineNinePatch)
        {
            VerticalStack vStack = new VerticalStack();
            Child = vStack;

            HorizontalStack hStack = null;

            for (int i = 0; i < 127; i++)
            {
                if ((i % 8) == 0)
                {
                    if (hStack != null)
                        vStack.Children.Add(hStack);

                    hStack = new HorizontalStack();
                }

                int controller = i + 1;

                hStack.Children.Add(new TextButton(controller.ToString())
                {
                    DesiredWidth = 100,
                    TextFont = Layout.Current.GetFont("SmallFont"),
                    ClickAction = delegate
                    {
                        StompboxClient.Instance.SendCommand("MapController " + controller + " " + pluginID + " " + parameterName);

                        if (CloseAction != null)
                        {
                            CloseAction();
                        }
                    }
                });
            }

            if (hStack != null)
                vStack.Children.Add(hStack);
        }

        public void Opened()
        {
        }
    }

    public class GainPluginInterface : PluginInterface
    {
        AudioLevelDisplay levelDisplay;

        public GainPluginInterface(IAudioPlugin plugin)
            : base(plugin)
        {
            HorizontalAlignment = EHorizontalAlignment.Left;

            MinWidth = 0;
        }

        protected override void AddControls(Dock dock)
        {
            ShowOutputParameters = false;

            showOptionsMenu = false;

            HorizontalStack stack = new HorizontalStack
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                ChildSpacing = 10
            };
            dock.Children.Add(stack);

            Dock controlDock = new Dock();
            stack.Children.Add(controlDock);

            base.AddControls(controlDock);

            levelDisplay = new AudioLevelDisplay() { DesiredHeight = 300, DesiredWidth = 40, VerticalAlignment = EVerticalAlignment.Center };

            stack.Children.Add(levelDisplay);
        }

        protected override void DrawContents()
        {
            levelDisplay.SetValue(Plugin.GetParameter("Level").Value);

            base.DrawContents();
        }
    }

    public class ParameterValueDisplay : NinePatchWrapper
    {
        public float HoldSeconds { get; set; }
        public float FadeSeconds { get; set; }
        public string ValueFormat { get; set; } = "0.0";

        float visibleSeconds = 0;
        StringBuilderTextBlock textBlock;
        double value = double.MinValue;

        public ParameterValueDisplay()
            : base(Layout.Current.GetImage("HoverTextOutline"))
        {
            Visible = false;

            HorizontalAlignment = EHorizontalAlignment.Center;
            VerticalAlignment = EVerticalAlignment.Center;

            HoldSeconds = 0.25f;
            FadeSeconds = 0.25f;

            textBlock = new StringBuilderTextBlock
            {
                TextColor = UIColor.Black,
                TextFont = Layout.Current.GetFont("SmallFont"),
                Margin = new LayoutPadding(5, 5),
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center
            };

            Child = textBlock;
        }

        public void SetValue(double value)
        {
            if (value != this.value)
            {
                this.value = value;

                textBlock.StringBuilder.Clear();
                textBlock.StringBuilder.AppendFormat(ValueFormat, value);
            }

            UpdateActive();
        }

        public void UpdateActive()
        {
            Visible = true;

            visibleSeconds = 0;

            Color = new UIColor((byte)Color.R, (byte)Color.G, (byte)Color.B, (byte)255);
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            if (Visible)
            {
                visibleSeconds += Layout.Current.SecondsElapsed;

                if (visibleSeconds > HoldSeconds)
                {
                    if (visibleSeconds > (HoldSeconds + FadeSeconds))
                    {
                        Visible = false;
                    }
                    else
                    {
                        byte alpha = (byte)(255 * MathUtil.Saturate(1.0f - ((visibleSeconds - HoldSeconds) / FadeSeconds)));

                        Color = new UIColor((byte)Color.R, (byte)Color.G, (byte)Color.B, alpha);
                    }
                }
            }
        }
    }

    public class EnumInterface : Dock
    {
        public Action<int> SelectionChangedAction { get; set; }
        public string NoSelectionText
        {
            get { return noSelectionText; }
            set
            {
                noSelectionText = value;

                if (selectedIndex == -1)
                {
                    button.Text = noSelectionText;

                    UpdateContentLayout();
                }
            }
        }
        public IList<string> EnumValues { get; private set; }

        public int SelectedIndex
        {
            get { return selectedIndex; }
        }

        public string SelectedIndexValue
        {
            get { return (selectedIndex == -1) ? null : menuItems[selectedIndex].Text; }
        }

        Menu menu;
        TextButton button;
        List<MenuItem> menuItems;
        int selectedIndex = -1;
        string noSelectionText = "---";

        public EnumInterface(IList<string> enumValues, UIColor textColor)
        {
            menuItems = new List<MenuItem>();

            menu = new Menu();

            SetEnumValues(enumValues);

            button = new TextButton("---")
            {
                TextColor = textColor,
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                ClickAction = delegate
                {
                    if (menuItems.Count > 0)
                        Layout.Current.ShowPopup(menu, button.ContentBounds.Center);
                }
            };

            Children.Add(button);
        }

        public void SetEnumValues(IList<string> enumValues)
        {
            this.EnumValues = enumValues;

            string selectedString = null;

            if ((selectedIndex >= 0) && (selectedIndex < menuItems.Count))
            {
                selectedString = menuItems[selectedIndex].Text;
            }
            else
            {
                selectedString = null;
            }

            if (menuItems.Count > 0)
            {
                menuItems.Clear();
            }

            int i = 0;

            foreach (string enumValue in enumValues)
            {
                int index = i++;

                menuItems.Add(new ContextMenuItem
                {
                    Text = enumValue,
                    CloseOnSelect = true,
                    AfterCloseAction = delegate
                    {
                        SetSelectedIndex(index);

                        if (SelectionChangedAction != null)
                            SelectionChangedAction(index);

                        UpdateContentLayout();
                    }
                });
            }

            menu.SetMenuItems(menuItems);

            if (selectedString != null)
            {
                int index = 0;

                foreach (ContextMenuItem item in menuItems)
                {
                    if (item.Text == selectedString)
                    {
                        selectedIndex = index;

                        break;
                    }

                    index++;
                }

                SetSelectedIndex(selectedIndex);
            }

            UpdateContentLayout();
        }

        public void SetSelectedIndex(int index)
        {
            selectedIndex = index;

            if ((index < 0) || (index >= menuItems.Count))
            {
                button.Text = NoSelectionText;
            }
            else
            {
                if (menuItems.Count > index)
                {
                    button.Text = menuItems[index].Text;
                }
            }
        }
    }

    public class FileInterface : Dock
    {
        public Action<int> SelectionChangedAction { get; set; }
        public string NoSelectionText
        {
            get { return noSelectionText; }
            set
            {
                noSelectionText = value;

                if (selectedIndex == -1)
                {
                    button.Text = noSelectionText;

                    UpdateContentLayout();
                }
            }
        }
        public IList<string> EnumValues { get; private set; }

        public int SelectedIndex
        {
            get { return selectedIndex - indexOffset; }
        }

        public string SelectedIndexValue
        {
            get { return (selectedIndex < indexOffset) ? null : menuItems[selectedIndex].Text; }
        }

        Menu menu;
        TextButton button;
        List<MenuItem> menuItems;
        int selectedIndex = -1;
        string noSelectionText = "---";
        int indexOffset = 0;

        public FileInterface(string filePath, IList<string> enumValues, UIColor textColor)
        {
            menuItems = new List<MenuItem>();

            menu = new Menu();

            SetEnumValues(filePath, enumValues);

            button = new TextButton(NoSelectionText)
            {
                TextColor = textColor,
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                ClickAction = delegate
                {
                    if (menuItems.Count > 0)
                        Layout.Current.ShowPopup(menu, button.ContentBounds.Center);
                }
            };

            Children.Add(button);
        }

        public void SetEnumValues(string filePath, IList<string> enumValues)
        {
            this.EnumValues = enumValues;

            string selectedString = null;

            if ((selectedIndex >= 0) && (selectedIndex < menuItems.Count))
            {
                selectedString = menuItems[selectedIndex].Text;
            }
            else
            {
                selectedString = null;
            }

            if (menuItems.Count > 0)
            {
                menuItems.Clear();
            }

            int i = 0;

            if (!string.IsNullOrEmpty(filePath))
            {
                menuItems.Add(new ContextMenuItem
                {
                    Text = "Open " + filePath + " Folder",
                    CloseOnSelect = true,
                    AfterCloseAction = delegate
                    {
                        try
                        {
                            System.Diagnostics.Process.Start("explorer.exe", Path.Combine(StompboxClient.Instance.PluginPath, filePath));
                        }
                        catch { }
                    }
                });

                indexOffset = 1;

                i++;
            }

            foreach (string enumValue in enumValues)
            {
                int index = i++;

                menuItems.Add(new ContextMenuItem
                {
                    Text = enumValue,
                    CloseOnSelect = true,
                    AfterCloseAction = delegate
                    {
                        SetSelectedIndex(index - indexOffset);

                        if (SelectionChangedAction != null)
                            SelectionChangedAction(index - indexOffset);

                        UpdateContentLayout();
                    }
                });
            }

            menu.SetMenuItems(menuItems);

            if (selectedString != null)
            {
                int index = 0;

                foreach (ContextMenuItem item in menuItems)
                {
                    if (item.Text == selectedString)
                    {
                        selectedIndex = index;

                        break;
                    }

                    index++;
                }

                SetSelectedIndex(selectedIndex);
            }

            UpdateContentLayout();
        }

        public void SetSelectedIndex(int index)
        {
            selectedIndex = index + indexOffset;

            if ((selectedIndex < indexOffset) || (selectedIndex >= menuItems.Count))
            {
                button.Text = "---";
            }
            else
            {
                if (menuItems.Count > selectedIndex)
                {
                    button.Text = menuItems[selectedIndex].Text;
                }
            }
        }
    }


    public class ParameterDial : Dock
    {
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float DefaultValue { get; set; }
        public float RangePower { get; set; } = 1.0f;
        public Action<float> ValueChangedAction { get; set; }
        public Action HoldAction { get; set; }

        ImageElement background;
        RotatingImageElement pointer;
        float currentValue;

        public ParameterDial()
        {
            MinValue = 0;
            MaxValue = 1;
            DefaultValue = 0.5f;

            background = new ImageElement("DialBackground") { HorizontalAlignment = EHorizontalAlignment.Center, VerticalAlignment = EVerticalAlignment.Center };
            Children.Add(background);

            pointer = new RotatingImageElement("DialPointer") { HorizontalAlignment = EHorizontalAlignment.Center, VerticalAlignment = EVerticalAlignment.Center, Color = UIColor.Black };
            Children.Add(pointer);

            SetValue(DefaultValue);
        }

        public void SetDialColor(UIColor color)
        {
            background.Color = color;
        }

        public void SetPointerColor(UIColor color)
        {
            pointer.Color = color;
        }

        public void SetValue(float value)
        {
            currentValue = MathUtil.Clamp(value, MinValue, MaxValue);

            float val = (currentValue - MinValue) / (MaxValue - MinValue);

            float maxAngle = 143;

            float angle = -maxAngle + (val * maxAngle * 2);

            pointer.Rotation = MathUtil.ToRadians(angle);
        }

        float touchStartValue;

        public override bool HandleTouch(in Touch touch)
        {
            switch (touch.TouchState)
            {
                case ETouchState.Pressed:
                    CaptureTouch(touch);
                    touchStartValue = currentValue;
                    break;
                case ETouchState.Moved:
                case ETouchState.Held:
                    if (HaveTouchCapture)
                    {
                        float delta = TouchCaptureStartPosition.Y - touch.Position.Y;

                        float range = MaxValue - MinValue;

                        float newValue = touchStartValue + ((delta * range) / 160);    //(double)PixGame.Instance.ScreenPPI);

                        newValue = MathUtil.Clamp(newValue, MinValue, MaxValue);

                        SetValue(newValue);

                        if (ValueChangedAction != null)
                            ValueChangedAction(newValue);
                    }
                    break;
                case ETouchState.Released:
                case ETouchState.Invalid:
                    ReleaseTouch();
                    break;
                default:
                    break;
            }

            if (IsDoubleTap(touch, this))
            {
                SetValue(DefaultValue);

                if (ValueChangedAction != null)
                    ValueChangedAction(DefaultValue);
            }

            return true;
        }

        //public override bool HandleGesture(PixGesture gesture)
        //{
        //    if (gesture.GestureType == EPixGestureType.Hold)
        //    {
        //        if (HoldAction != null)
        //        {
        //            HoldAction();

        //            return true;
        //        }
        //    }

        //    return base.HandleGesture(gesture);
        //}
    }

    public class PowerLevel : Dock
    {
        ImageElement onElement;

        public PowerLevel()
        {
            Children.Add(new ImageElement("PowerOff"));
            Children.Add(onElement = new ImageElement("PowerOn"));

            onElement.Color = UIColor.Transparent;
        }

        public void SetLevel(float level)
        {
            onElement.Color = new UIColor(1.0f, 1.0f, 1.0f, level);
        }
    }


    public class PowerButton : Button
    {
        public Action HoldAction { get; set; }

        public PowerButton()
        {
            PressedElement = new ImageElement("PowerOn");
            UnpressedElement = new ImageElement("PowerOff");

            IsToggleButton = true;
        }
    }

    public class AudioLevelDisplay : Dock
    {
        public double WarnLevel { get; set; }

        ImageElement activeLevelImage;
        double lastValue = 0;
        double clip = 0;

        public AudioLevelDisplay()
        {
            WarnLevel = 0.8f;
            BackgroundColor = UIColor.Black;
            HorizontalAlignment = EHorizontalAlignment.Stretch;
            VerticalAlignment = EVerticalAlignment.Stretch;

            Children.Add(new ImageElement("LevelDisplay")
            {
                Color = new UIColor(20, 20, 20),
            });

            activeLevelImage = new ImageElement("LevelDisplay")
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Bottom,
                Color = UIColor.Green,
            };
            activeLevelImage.Visible = false;
            Children.Add(activeLevelImage);
        }

        public void SetValue(double value)
        {
            if (value < 0.01)
                value = 0;

            if (clip > 0)
            {
                clip -= 0.1;

                if (clip <= 0)
                    activeLevelImage.Color = UIColor.Green;
            }

            if (value >= 1.0)
            {
                clip = 1;
                activeLevelImage.Color = UIColor.Red;
            }
            else if (value >= WarnLevel)
            {
                clip = 1;
                activeLevelImage.Color = UIColor.Orange;
            }

            if (value != lastValue)
            {
                lastValue = value;

                double logLevel = Math.Min(Math.Log10((value * 9.0) + 1.0), 1.0);

                int height = (int)((double)activeLevelImage.Image.Height * logLevel);

                if (height > activeLevelImage.Image.Height)
                    height = activeLevelImage.Image.Height;

                activeLevelImage.SourceRectangle = new Rectangle(0, activeLevelImage.Image.Height - height, activeLevelImage.Image.Width, height);
                activeLevelImage.DesiredHeight = ContentBounds.Height * (float)logLevel;
                activeLevelImage.Visible = (logLevel > 0);

                UpdateContentLayout();
            }
        }
    }
}
