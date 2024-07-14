using System;
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
        Enum
    };

    public class PluginParameter
    {
        public IAudioPlugin Plugin { get; set; }
        public string Name { get; set; }
        public string ValueFormat { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double DefaultValue { get; set; }
        public EParameterType ParameterType { get; set; }
        public bool CanSyncToHostBPM { get; set; }
        public int HostBPMSyncNumerator { get; set; }
        public int HostBPMSyncDenominator { get; set; }
        public bool IsAdvanced { get; set; }
        public bool IsVisible { get; set; }
        public virtual double Value
        {
            get { return (GetValue != null) ? GetValue() : value; }
            set
            {
                if (SetValue != null)
                    SetValue(value);

                this.value = value;

                if (Plugin.StompboxClient.InClientMode && !Plugin.StompboxClient.SuppressCommandUpdates)
                {
                    if (CanSyncToHostBPM && (HostBPMSyncNumerator != 0) && (HostBPMSyncDenominator != 0))
                    {
                        Plugin.StompboxClient.SendCommand("SetParam " + Plugin.ID + " " + Name + " " + HostBPMSyncNumerator + "/" + HostBPMSyncDenominator);
                    }
                    else
                    {
                        Plugin.StompboxClient.SendCommand("SetParam " + Plugin.ID + " " + Name + " " +
                            ((ParameterType == EParameterType.Enum) ? ((EnumValues.Length > 0) ? EnumValues[(int)Value] : "") : Value.ToString()));
                    }
                }
            }
        }
        public string DisplayValue { get { return String.Format(ValueFormat, Value); } }
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
        public Func<double> GetValue { get; set; }
        public Action<double> SetValue { get; set; }
        public Action UpdateBPMSync { get; set; }

        double value;

        public PluginParameter()
        {
            IsVisible = true;
            ValueFormat = "{0:0.00}";
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
        StompboxClient StompboxClient { get; set; }
        String Name { get; set; }
        String ID { get; }
        bool Enabled { get; set; }
        double OutputValue { get; set; }
        bool EnabledIsSwitchable { get; set; }
        String BackgroundColor { get; set; }
        String ForegroundColor { get; set; }
        bool IsUserSelectable { get; set; }
        double ParameterTextSize { get; set; }
        string DialStyle { get; set; }
        ObservableCollection<PluginParameter> Parameters { get; set; }
        PluginParameter EnumParameter { get; set; }
        Action<bool> SetEnabled { get; set; }
        Action<double> SetOutputValue { get; set; }

        PluginParameter GetParameter(string parameterName);
    }

    public class AudioPluginBase : IAudioPlugin
    {
        public StompboxClient StompboxClient { get; set; }
        public String Name { get; set; }
        public String ID { get; set; }
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

                if (StompboxClient.InClientMode && !StompboxClient.SuppressCommandUpdates)
                {
                    StompboxClient.SendCommand("SetParam " + ID + " Enabled " + (enabled ? "1" : "0"));
                }
            }
        }
        public virtual double OutputValue
        {
            get
            {
                return outputValue;
            }
            set
            {
                outputValue = value;

                if (SetOutputValue != null)
                {
                    SetOutputValue(value);
                }
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
        public Action<double> SetOutputValue { get; set; }

        protected bool enabled;
        double outputValue;

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

    public class AudioPluginChain : AudioPluginBase, IAudioPlugin
    {
        public ObservableCollection<IAudioPlugin> Plugins { get; }
        public override bool Enabled
        {
            get { return base.Enabled && (NumActivePlugins() > 0); }
            set { base.Enabled = value; }
        }

        public AudioPluginChain()
        {
            enabled = true;
            Plugins = new ObservableCollection<IAudioPlugin>();
        }

        public int NumActivePlugins()
        {
            int numActiveEffects = 0;

            foreach (IAudioPlugin plugin in Plugins)
            {
                if (plugin.Enabled)
                    numActiveEffects++;
            }

            return numActiveEffects;
        }

        public override string ToString()
        {
            return string.Join("-", Plugins);
        }
    }
}