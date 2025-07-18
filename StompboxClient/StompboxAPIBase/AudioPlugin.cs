﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Stompbox
{
    public enum EParameterType
    {
        Knob,
        Bool,
        Int,
        VSlider,
        Enum,
        File,
        Power
    };

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

    public class PluginParameter
    {
        public IAudioPlugin Plugin { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ValueFormat { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float DefaultValue { get; set; }
        public float RangePower { get; set; } = 1;
        public EParameterType ParameterType { get; set; }
        public bool CanSyncToHostBPM { get; set; }
        public int HostBPMSyncNumerator { get; set; }
        public int HostBPMSyncDenominator { get; set; }
        public bool IsAdvanced { get; set; }
        public bool IsOutput { get; set; }
        public bool IsVisible { get; set; }
        public virtual float Value { get; set; }
        public float NormalizedValue
        {
            get
            {
                return GetNormalizedValue(Value);
            }
            set
            {
                Value = GetRangeValue(value);
            }
        }
        public string DisplayValue { get { return String.Format(ValueFormat, Value); } }
        public String FilePath { get; set; } = null;
        public String[] EnumValues { get; set; }
        public int[] IntValues { get; set; }
        public int IntValue
        {
            get
            {
                return (int)Value;
            }
            set
            {
                Value = value;
            }
        }
        public Func<float> GetValue { get; set; }
        public Action<float> SetValue { get; set; }
        public Action UpdateBPMSync { get; set; }

        double value;

        public PluginParameter()
        {
            IsVisible = true;
            ValueFormat = "{0:0.00}";
        }

        public float GetNormalizedValue(float value)
        {
            float val = (value - MinValue) / (MaxValue - MinValue);

            return (float)((RangePower < 0) ? (1 - (Math.Pow(1 - val, 1 / -RangePower))) : Math.Pow(val, 1 / RangePower));
        }

        public float GetRangeValue(float normalizedValue)
        {
            float val = (float)((RangePower < 0) ? (1 - Math.Pow(1 - normalizedValue, -RangePower)) : Math.Pow(normalizedValue, RangePower));

            return MinValue + ((MaxValue - MinValue) * val);
        }

        public override string ToString()
        {
            return Name + ": " + DisplayValue;
        }

        public PluginParameter ShallowCopy()
        {
            return (PluginParameter)this.MemberwiseClone();
        }
    }

    public interface IAudioPlugin
    {
        String Name { get; set; }
        String ID { get; }
        String Description { get; set; }
        bool Enabled { get; set; }
        bool EnabledIsSwitchable { get; set; }
        String BackgroundColor { get; set; }
        String ForegroundColor { get; set; }
        bool IsUserSelectable { get; set; }
        double ParameterTextSize { get; set; }
        string DialStyle { get; set; }
        ObservableCollection<PluginParameter> Parameters { get; set; }
        PluginParameter EnumParameter { get; set; }
        Action<bool> SetEnabled { get; set; }

        PluginParameter GetParameter(string parameterName);
    }

    public class AudioPluginBase : IAudioPlugin
    {
        public String Name { get; set; }
        public String ID { get; set; }
        public String Description { get; set; }
        public virtual bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;

                if (SetEnabled != null)
                    SetEnabled(enabled);
            }
        }
        public String BackgroundColor { get; set; }
        public String ForegroundColor { get; set; }
        public bool IsUserSelectable { get; set; }
        public double ParameterTextSize { get; set; }
        public string DialStyle { get; set; }
        public bool EnabledIsSwitchable { get; set; }
        public ObservableCollection<PluginParameter> Parameters { get; set; }
        public PluginParameter EnumParameter { get; set; }
        public Action<bool> SetEnabled { get; set; }

        protected bool enabled;
        float outputValue;

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool initialized = false;

        public AudioPluginBase()
        {
            EnabledIsSwitchable = true;
            Parameters = new ObservableCollection<PluginParameter>();

            ForegroundColor = "#000000";
            BackgroundColor = "#c9c9c9";
            ParameterTextSize = 11;
        }

        public override string ToString()
        {
            return ID; 
        }

        public PluginParameter GetParameter(string parameterName)
        {
            foreach (PluginParameter parameter in Parameters)
            {
                if (parameter.Name == parameterName)
                    return parameter;
            }

            return null;
        }
    }
}