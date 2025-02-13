using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace StompboxAPI
{
    public class BPMSync
    {
        public static List<BPMSync> Timings;

        public string Name { get; set; }
        public int Numerator { get; set; }
        public int Denomenator { get; set; }

        static BPMSync()
        {
            Timings = new List<BPMSync>
            {
                new BPMSync
                {
                    Name = "Custom",
                    Numerator = 0,
                    Denomenator = 0
                },
                new BPMSync
                {
                    Name = "Half Note",
                    Numerator = 2,
                    Denomenator = 1
                },
                new BPMSync
                {
                    Name = "Dotted 1/4 Note",
                    Numerator = 3,
                    Denomenator = 2
                },
                new BPMSync
                {
                    Name = "1/4 Note",
                    Numerator = 1,
                    Denomenator = 1
                },
                new BPMSync
                {
                    Name = "Dotted 1/8th",
                    Numerator = 3,
                    Denomenator = 4
                },
                new BPMSync
                {
                    Name = "Triplet of Half",
                    Numerator = 2,
                    Denomenator = 3
                },
                new BPMSync
                {
                    Name = "1/8th Note",
                    Numerator = 1,
                    Denomenator = 2
                },
                new BPMSync
                {
                    Name = "Dotted 1/16th",
                    Numerator = 3,
                    Denomenator = 8
                },
                new BPMSync
                {
                    Name = "Triplet of Quarter",
                    Numerator = 1,
                    Denomenator = 3
                },
                new BPMSync
                {
                    Name = "16th Note",
                    Numerator = 1,
                    Denomenator = 4
                }
            };
        }
    }

    public class ProtocolClient
    {
        public List<String> PluginNames { get; private set; }

        Dictionary<string, IAudioPlugin> pluginDefs = new Dictionary<string, IAudioPlugin>();

        StompboxClient StompboxClient;

        public ProtocolClient(StompboxClient StompboxClient)
        {
            this.StompboxClient = StompboxClient;
             
            PluginNames = new List<string>();

            // Force invariant locale so we don't have issues parsing decimal places in locales that use ','
            var culture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        public IAudioPlugin GetPluginDefinition(string pluginName)
        {
            if (pluginDefs.ContainsKey(pluginName))
                return pluginDefs[pluginName];

            return null;
        }

        public IAudioPlugin CreateNewPlugin(string pluginName, string pluginID)
        {
            if (!pluginDefs.ContainsKey(pluginName))
            {
                throw new InvalidOperationException();
            }

            IAudioPlugin plugin = new AudioPluginBase { ID = pluginID, Name = pluginName };

            if (plugin.Parameters.Count == 0)
            {
                if (pluginDefs.ContainsKey(plugin.Name))
                {
                    IAudioPlugin pluginDef = pluginDefs[plugin.Name];

                    plugin.Description = pluginDef.Description;
                    plugin.BackgroundColor = pluginDef.BackgroundColor;
                    plugin.ForegroundColor = pluginDef.ForegroundColor;
                    plugin.IsUserSelectable = pluginDef.IsUserSelectable;
                    plugin.StompboxClient = StompboxClient;

                    foreach (PluginParameter paramDef in pluginDef.Parameters)
                    {
                        PluginParameter parameter = paramDef.ShallowCopy();
                        parameter.Plugin = plugin;

                        StompboxClient.SuppressCommandUpdates = true;
                        parameter.Value = parameter.DefaultValue;
                        StompboxClient.SuppressCommandUpdates = false;

                        plugin.Parameters.Add(parameter);

                        if ((parameter.ParameterType == EParameterType.Enum) || (paramDef.ParameterType == EParameterType.File))
                            plugin.EnumParameter = parameter;

                        if (parameter.CanSyncToHostBPM)
                        {
                            parameter.UpdateBPMSync = delegate
                            {
                                StompboxClient.SuppressCommandUpdates = true;

                                if ((parameter.HostBPMSyncNumerator != 0) && (parameter.HostBPMSyncDenominator != 0))
                                {
                                    parameter.Value = ((60.0 / StompboxClient.BPM) * ((double)parameter.HostBPMSyncNumerator / (double)parameter.HostBPMSyncDenominator)) * 1000;
                                }
                                else
                                {
                                    parameter.Value = parameter.DefaultValue;
                                }

                                StompboxClient.SuppressCommandUpdates = false;
                            };
                        }
                    }
                }
            }

            return plugin;
        }

        public void ReleasePlugin(IAudioPlugin plugin)
        {
            StompboxClient.PluginFactory.ReleasePlugin(plugin);

            StompboxClient.SendCommand("ReleasePlugin " + plugin.ID);
        }

        char[] split = { ' ' };

        public void HandleCommand(string cmd)
        {
            StompboxClient.Debug("** CMD: " + cmd);

            try
            {
                string[] cmdWords = Regex.Matches(cmd, @"(['\""])(?<value>.+?)\1|(?<value>[^ ]+)")
                    .Cast<Match>()
                    .Select(m => m.Groups["value"].Value)
                    .ToArray();


                //Regex.Matches(cmd, @"[\""].+?[\""]|[^ ]+")
                //.Cast<Match>()
                //.Select(m => m.Value)
                //.ToArray();

                if (cmdWords.Length > 0)
                {
                    switch (cmdWords[0])
                    {
                        case "Presets":
                            int numPresets = cmdWords.Length - 1;

                            if (numPresets > 0)
                            {
                                List<string> presets = new List<string>();

                                for (int i = 0; i < numPresets; i++)
                                {
                                    presets.Add(cmdWords[i + 1]);
                                }

                                presets.Sort();

                                StompboxClient.SetPresetNames(presets);

                            }
                            break;

                        case "SetPreset":
                            if (cmdWords.Length > 1)
                            {
                                StompboxClient.MidiCCMap.Clear();

                                StompboxClient.SuppressCommandUpdates = true;
                                StompboxClient.SetSelectedPreset(cmdWords[1]);
                                StompboxClient.SuppressCommandUpdates = false;
                            }
                            break;

                        case "SetChain":
                            if (cmdWords.Length > 1)
                            {
                                List<IAudioPlugin> plugins = new List<IAudioPlugin>();

                                int numPlugins = cmdWords.Length - 2;

                                for (int i = 0; i < numPlugins; i++)
                                {
                                    string pluginID = cmdWords[i + 2];

                                    IAudioPlugin plugin = StompboxClient.PluginFactory.CreatePlugin(pluginID);

                                    plugins.Add(plugin);
                                }

                                switch (cmdWords[1])
                                {
                                    case "Input":
                                        StompboxClient.SetInputChain(plugins);
                                        break;
                                    case "FxLoop":
                                        StompboxClient.SetFxLoop(plugins);
                                        break;
                                    case "Output":
                                        StompboxClient.SetOutputChain(plugins);
                                        break;
                                }
                            }

                            break;

                        case "SetPluginSlot":
                            if (cmdWords.Length > 2)
                            {
                                StompboxClient.SetSlotPlugin(cmdWords[1], cmdWords[2]);
                            }
                            break;

                        case "SetParam":
                            if (cmdWords.Length > 3)
                            {
                                IAudioPlugin plugin = StompboxClient.PluginFactory.CreatePlugin(cmdWords[1]);

                                if (plugin != null)
                                {
                                    if (cmdWords[2] == "Enabled")
                                    {
                                        int enabled = 0;

                                        int.TryParse(cmdWords[3], out enabled);

                                        StompboxClient.SuppressCommandUpdates = true;
                                        plugin.Enabled = (enabled == 1);
                                        StompboxClient.SuppressCommandUpdates = false;
                                    }
                                    else
                                    {
                                        foreach (PluginParameter parameter in plugin.Parameters)
                                        {
                                            if (parameter.Name == cmdWords[2])
                                            {
                                                double value = parameter.DefaultValue;

                                                if (parameter.CanSyncToHostBPM)
                                                {
                                                    parameter.HostBPMSyncNumerator = parameter.HostBPMSyncDenominator = 0;
                                                }

                                                if (parameter.ParameterType == EParameterType.Enum)
                                                {
                                                    int pos = 0;

                                                    foreach (string enumValue in parameter.EnumValues)
                                                    {
                                                        if (enumValue == cmdWords[3])
                                                        {
                                                            value = pos;

                                                            break;
                                                        }

                                                        pos++;
                                                    }
                                                }
                                                else if (parameter.ParameterType == EParameterType.File)
                                                {
                                                    int pos = 0;

                                                    foreach (string enumValue in parameter.EnumValues)
                                                    {
                                                        if (enumValue == cmdWords[3])
                                                        {
                                                            value = pos;

                                                            break;
                                                        }

                                                        pos++;
                                                    }
                                                }
                                                else if (parameter.CanSyncToHostBPM && (cmdWords[3].IndexOf('/') != -1))
                                                {
                                                    string[] numDenom = cmdWords[3].Split('/');

                                                    int numerator = 0;
                                                    int denominator = 0;

                                                    if (numDenom.Length > 1)
                                                    {
                                                        int.TryParse(numDenom[0], out numerator);
                                                        int.TryParse(numDenom[1], out denominator);

                                                        parameter.HostBPMSyncNumerator = numerator;
                                                        parameter.HostBPMSyncDenominator = denominator;

                                                        if ((numerator != 0) && (denominator != 0))
                                                        {
                                                            value = ((60.0 / StompboxClient.BPM) * ((double)parameter.HostBPMSyncNumerator / (double)parameter.HostBPMSyncDenominator)) * 1000;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    double.TryParse(cmdWords[3], out value);
                                                }

                                                StompboxClient.SuppressCommandUpdates = true;
                                                parameter.Value = value;
                                                StompboxClient.SuppressCommandUpdates = false;

                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            break;

                        case "SetOutput":
                            if (cmdWords.Length > 2)
                            {
                                IAudioPlugin plugin = StompboxClient.PluginFactory.GetPlugin(cmdWords[1]);

                                if (plugin != null)
                                {
                                    double outputValue = 0;

                                    if (double.TryParse(cmdWords[2], out outputValue))
                                    {
                                        plugin.OutputValue = outputValue;
                                    }
                                }

                                break;
                            }
                            break;

                        case "DSPLoad":
                            if (cmdWords.Length > 2)
                            {
                                float maxLoad = 0;
                                float.TryParse(cmdWords[1], out maxLoad);

                                float minLoad = 0;
                                float.TryParse(cmdWords[2], out minLoad);


                                StompboxClient.ReportDSPLoad(maxLoad, minLoad);
                            }
                            break;

                        case "PluginConfig":
                            if (cmdWords.Length > 1)
                            {
                                string pluginName = cmdWords[1];

                                AudioPluginBase pluginDef = new AudioPluginBase { Name = pluginName };
                                pluginDefs[pluginName] = pluginDef;

                                int numProps = (cmdWords.Length - 2) / 2;

                                for (int prop = 0; prop < numProps; prop++)
                                {
                                    string propName = cmdWords[(prop * 2) + 2];
                                    string propValue = cmdWords[(prop * 2) + 3];

                                    switch (propName)
                                    {
                                        case "BackgroundColor":
                                            pluginDef.BackgroundColor = propValue;
                                            break;
                                        case "ForegroundColor":
                                            pluginDef.ForegroundColor = propValue;
                                            break;
                                        case "IsUserSelectable":
                                            int isSelectable = 0;

                                            int.TryParse(propValue, out isSelectable);

                                            pluginDef.IsUserSelectable = (isSelectable == 1);
                                            break;

                                        case "Description":
                                            pluginDef.Description = propValue;
                                            break;
                                    }
                                }

                                if (pluginDef.IsUserSelectable)
                                    PluginNames.Add(pluginDef.Name);

                            }

                            break;
                        case "ParameterConfig":
                            if (cmdWords.Length > 2)
                            {
                                if (pluginDefs.ContainsKey(cmdWords[1]))
                                {
                                    IAudioPlugin pluginDef = pluginDefs[cmdWords[1]];

                                    PluginParameter newParameter = new PluginParameter() { Name = cmdWords[2] };

                                    int numProps = (cmdWords.Length - 3) / 2;

                                    for (int prop = 0; prop < numProps; prop++)
                                    {
                                        string propName = cmdWords[(prop * 2) + 3];
                                        string propValue = cmdWords[(prop * 2) + 4];

                                        switch (propName)
                                        {
                                            case "Type":
                                                EParameterType paramType;
                                                Enum.TryParse<EParameterType>(propValue, out paramType);
                                                newParameter.ParameterType = paramType;
                                                break;
                                            case "MinValue":
                                                double minValue = 0;
                                                Double.TryParse(propValue, out minValue);
                                                newParameter.MinValue = minValue;
                                                break;
                                            case "MaxValue":
                                                double maxValue = 0;
                                                Double.TryParse(propValue, out maxValue);
                                                newParameter.MaxValue = maxValue;
                                                break;
                                            case "RangePower":
                                                double rangePower = 0;
                                                Double.TryParse(propValue, out rangePower);
                                                newParameter.RangePower = rangePower;
                                                break;
                                            case "DefaultValue":
                                                double defaultValue = 0;
                                                Double.TryParse(propValue, out defaultValue);
                                                newParameter.DefaultValue = defaultValue;
                                                break;
                                            case "ValueFormat":
                                                newParameter.ValueFormat = propValue;
                                                break;
                                            case "CanSyncToHostBPM":
                                                int canSyncToHostBPM = 0;
                                                int.TryParse(propValue, out canSyncToHostBPM);
                                                newParameter.CanSyncToHostBPM = (canSyncToHostBPM == 1);
                                                break;
                                            case "IsAdvanced":
                                                int isAdvanced = 0;
                                                int.TryParse(propValue, out isAdvanced);
                                                newParameter.IsAdvanced = (isAdvanced == 1);
                                                break;
                                            case "Description":
                                                newParameter.Description = propValue;
                                                break;
                                        }
                                    }
                                    pluginDef.Parameters.Add(newParameter);
                                }
                            }
                            break;

                        case "ParameterEnumValues":
                            if (cmdWords.Length > 2)
                            {
                                if (pluginDefs.ContainsKey(cmdWords[1]))
                                {
                                    IAudioPlugin pluginDef = pluginDefs[cmdWords[1]];

                                    foreach (PluginParameter parameter in pluginDef.Parameters)
                                    {
                                        if (parameter.Name == cmdWords[2])
                                        {
                                            int numEnums = cmdWords.Length - 3;

                                            parameter.EnumValues = new string[numEnums];

                                            for (int i = 0; i < numEnums; i++)
                                            {
                                                parameter.EnumValues[i] = cmdWords[i + 3];
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case "ParameterFileTree":
                            if (cmdWords.Length > 3)
                            {
                                if (pluginDefs.ContainsKey(cmdWords[1]))
                                {
                                    IAudioPlugin pluginDef = pluginDefs[cmdWords[1]];

                                    foreach (PluginParameter parameter in pluginDef.Parameters)
                                    {
                                        if (parameter.Name == cmdWords[2])
                                        {
                                            parameter.FilePath = cmdWords[3];

                                            int numEnums = cmdWords.Length - 4;

                                            parameter.EnumValues = new string[numEnums];

                                            for (int i = 0; i < numEnums; i++)
                                            {
                                                parameter.EnumValues[i] = cmdWords[i + 4];
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case "MapController":
                            if (cmdWords.Length > 3)
                            {
                                StompboxClient.Instance.MidiCCMap.Add(new MidiCCMapEntry()
                                {
                                    CCNumber = int.Parse(cmdWords[1]),
                                    PluginName = cmdWords[2],
                                    PluginParameter = cmdWords[3]
                                });
                            }
                            break;

                        case "MapModeController":
                            if (cmdWords.Length > 1)
                            {
                                StompboxClient.MidiModeCC = int.Parse(cmdWords[1]);
                            }
                            break;

                        case "MapStompController":
                            if (cmdWords.Length > 2)
                            {
                                int stomp = int.Parse(cmdWords[1]);
                                int cc = int.Parse(cmdWords[2]);

                                StompboxClient.MidiStompCCMap[cc] = stomp;
                            }
                            break;

                        case "EndProgram":
                            StompboxClient.UpdateUI();
                            break;

                        case "Ok":
                            break;

                        default:    // Unrecognized command
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // In case we get an exception after setting it
                StompboxClient.SuppressCommandUpdates = false;

                StompboxClient.Debug("Exception: " + ex.ToString());
            }
        }
    }
}
