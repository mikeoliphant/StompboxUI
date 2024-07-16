using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnmanagedPlugins;

namespace Stompbox
{
    public class UnmanagedPluginParameter : PluginParameter
    {
        public override double Value
        {
            get
            {
                return base.GetValue();
            }

            set
            {
                if (SetValue != null)
                    SetValue(value);

                (Plugin as UnmanagedAudioPlugin).UnmanagedWrapper.SetParameter(ParameterIndex, value);
            }
        }

        public int ParameterIndex { get; set; }
    }

    public unsafe class UnmanagedAudioPlugin : AudioPluginBase, IAudioPlugin
    {
        public override bool Enabled
        {
            get
            {
                return UnmanagedWrapper.GetEnabled();
            }
            set
            {
                UnmanagedWrapper.SetEnabled(value);

                //GuitarSimPlugin.Instance.SendCommand("SetParam " + ID + " Enabled " + (value ? "1" : "0"));
            }
        }

        public override double OutputValue
        {
            get
            {
                return UnmanagedWrapper.GetOutputValue();
            }
        }

        public PluginWrapper UnmanagedWrapper
        {
            get; protected set;
        }

        Dictionary<string, double> cachedParameters = new Dictionary<string, double>();

        public UnmanagedAudioPlugin(PluginWrapper unmanagedWrapper)
        {
            SetUnmanagedWrapper(unmanagedWrapper);
        }

        public virtual void SetUnmanagedWrapper(PluginWrapper unmanagedWrapper)
        {
            this.UnmanagedWrapper = unmanagedWrapper;

            if (unmanagedWrapper != null)
            {
                ID = unmanagedWrapper.GetID();

                foreach (PluginParameter parameter in Parameters)
                {
                    cachedParameters[parameter.Name] = (parameter.Value - parameter.MinValue) / (parameter.MaxValue - parameter.MinValue);
                }

                ObservableCollection<PluginParameter> newParameters = new ObservableCollection<PluginParameter>();

                if (String.IsNullOrEmpty(Name))
                    Name = unmanagedWrapper.GetName();

                IsUserSelectable = unmanagedWrapper.GetIsUserSelectable();

                string backgroundColor = unmanagedWrapper.GetBackgroundColor();

                if (!string.IsNullOrEmpty(backgroundColor))
                {
                    BackgroundColor = backgroundColor;
                    //PaintColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(backgroundColor);
                }

                string foregroundColor = unmanagedWrapper.GetForegroundColor();

                if (!string.IsNullOrEmpty(foregroundColor))
                {
                    ForegroundColor = foregroundColor;
                }

                for (int i = 0; i < unmanagedWrapper.GetNumParameters(); i++)
                {
                    int index = i;

                    string paramName = unmanagedWrapper.GetParameterName(i);
                    EParameterType paramType = (EParameterType)unmanagedWrapper.GetParameterType(i);

                    String[] enumValues = null;

                    if (paramType == EParameterType.Enum)
                    {
                        enumValues = UnmanagedWrapper.GetParameterEnumValues(i);
                    }

                    int[] intValues = null;                    

                    if (paramType == EParameterType.Int)
                    {
                        int minVal = (int)UnmanagedWrapper.GetParameterMinValue(i);
                        int maxVal = (int)UnmanagedWrapper.GetParameterMaxValue(i);

                        if (minVal == maxVal)
                            continue;

                        intValues = new int[(maxVal - minVal) + 1];

                        for (int intVal = 0; intVal < intValues.Length; intVal++)
                        {
                            intValues[intVal] = minVal + intVal;
                        }
                    }

                    PluginParameter parameter = new UnmanagedPluginParameter
                    {
                        Plugin = this,
                        ParameterIndex = index,
                        Name = paramName,
                        MinValue = unmanagedWrapper.GetParameterMinValue(i),
                        MaxValue = unmanagedWrapper.GetParameterMaxValue(i),
                        DefaultValue = unmanagedWrapper.GetParameterDefaultValue(i),
                        ParameterType = paramType,
                        IsAdvanced = unmanagedWrapper.GetParameterIsAdvanced(i),
                        CanSyncToHostBPM = unmanagedWrapper.GetParameterCanSyncToHostBPM(i),
                        HostBPMSyncNumerator = unmanagedWrapper.GetParameterBPMSyncNumerator(i),
                        HostBPMSyncDenominator = unmanagedWrapper.GetParameterBPMSyncDenominator(i),
                        ValueFormat = unmanagedWrapper.GetParameterDisplayFormat(i),
                        EnumValues = enumValues,
                        IntValues = intValues,
                        GetValue = delegate
                        {
                            return unmanagedWrapper.GetParameter(index);
                        },
                    };

                    if (parameter.CanSyncToHostBPM)
                    {
                        parameter.UpdateBPMSync = delegate
                        {
                            unmanagedWrapper.SetParameterBPMSyncNumerator(index, parameter.HostBPMSyncNumerator);
                            unmanagedWrapper.SetParameterBPMSyncDenominator(index, parameter.HostBPMSyncDenominator);
                        };
                    }

                    if (paramType == EParameterType.Enum)
                    {
                        EnumParameter = parameter;
                    }

                    if (cachedParameters.ContainsKey(parameter.Name))
                    {
                        parameter.Value = parameter.MinValue + ((parameter.MaxValue - parameter.MinValue) * cachedParameters[parameter.Name]);

                        //Logger.Log("Set cached " + Name + "/" + unmanagedWrapper.GetParameterName(index) + " to " + parameter.Value );
                    }

                    newParameters.Add(parameter);
                }

                Parameters = newParameters;

                // Currently just for EQ-7...
                if (Parameters.Count > 6)
                {
                    ParameterTextSize = 9;
                }
            }

            //Logger.Log("Unmanaged wrapper changed for: " + Name);
        }
    }

    public class NamedAction
    {
        public string Name { get; set; }
        public Action Action { get; set; }

        public NamedAction(string name, Action action)
        {
            Name = name;
            Action = action;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
