using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Stompbox;

namespace StompboxAPI
{
    public class UnmanagedPluginParameter : PluginParameter
    {
        internal IntPtr nativeParameter;

        public override float Value
        {
            get
            {
                return NativeApi.GetParameterValue(nativeParameter);
            }

            set
            {
                if (SetValue != null)
                    SetValue(value);

                NativeApi.SetParameterValue(nativeParameter, value);
            }
        }

        public IntPtr ParameterHandle { get; set; }
    }

    public class UnmanagedAudioPlugin : AudioPluginBase, IAudioPlugin
    {
        internal IntPtr nativePlugin;

        public override bool Enabled
        {
            get { return NativeApi.GetPluginEnabled(nativePlugin); }
            set { NativeApi.SetPluginEnabled(nativePlugin, value); }
        }

        public override float OutputValue
        {
            get
            {
                return NativeApi.GetOutputValue(nativePlugin);
            }
        }

        Dictionary<string, float> cachedParameters = new Dictionary<string, float>();

        internal void SetNativePlugin(IntPtr nativePlugin)
        {
            this.nativePlugin = nativePlugin;

            ID = Marshal.PtrToStringAnsi(NativeApi.GetPluginID(nativePlugin));

            foreach (PluginParameter parameter in Parameters)
            {
                cachedParameters[parameter.Name] = (parameter.Value - parameter.MinValue) / (parameter.MaxValue - parameter.MinValue);
            }

            ObservableCollection<PluginParameter> newParameters = new ObservableCollection<PluginParameter>();

            if (String.IsNullOrEmpty(Name))
                Name = Marshal.PtrToStringAnsi(NativeApi.GetPluginName(nativePlugin));

            Description = Marshal.PtrToStringAnsi(NativeApi.GetPluginDescription(nativePlugin));

            IsUserSelectable = NativeApi.GetPluginIsUserSelectable(nativePlugin);

            string backgroundColor = Marshal.PtrToStringAnsi(NativeApi.GetPluginBackgroundColor(nativePlugin));

            if (!string.IsNullOrEmpty(backgroundColor))
            {
                BackgroundColor = backgroundColor;
                //PaintColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(backgroundColor);
            }

            string foregroundColor = Marshal.PtrToStringAnsi(NativeApi.GetPluginForegroundColor(nativePlugin));

            if (!string.IsNullOrEmpty(foregroundColor))
            {
                ForegroundColor = foregroundColor;
            }

            uint numParameters = NativeApi.GetPluginNumParameters(nativePlugin);

            for (uint i = 0; i < numParameters; i++)
            { 
                IntPtr nativeParameter = NativeApi.GetPluginParameter(nativePlugin, i);

                string paramName = Marshal.PtrToStringAnsi(NativeApi.GetParameterName(nativeParameter));
                EParameterType paramType = (EParameterType)NativeApi.GetParameterType(nativeParameter);

                string filePath = null;

                if (paramType == EParameterType.File)
                {
                    filePath = Marshal.PtrToStringAnsi(NativeApi.GetParameterFilePath(nativeParameter));
                }

                String[] enumValues = null;

                if ((paramType == EParameterType.Enum) || (paramType == EParameterType.File))
                {
                    enumValues = NativeApi.GetListFromStringVector(NativeApi.GetParameterEnumValues(nativeParameter)).ToArray();
                }

                int[] intValues = null;

                if (paramType == EParameterType.Int)
                {
                    int minVal = (int)NativeApi.GetParameterMinValue(nativeParameter);
                    int maxVal = (int)NativeApi.GetParameterMaxValue(nativeParameter);

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
                    nativeParameter = nativeParameter,
                    Name = paramName,
                    Description = Marshal.PtrToStringAnsi(NativeApi.GetParameterDescription(nativeParameter)),
                    MinValue = NativeApi.GetParameterMinValue(nativeParameter),
                    MaxValue = NativeApi.GetParameterMaxValue(nativeParameter),
                    DefaultValue = NativeApi.GetParameterDefaultValue(nativeParameter),
                    RangePower = NativeApi.GetParameterRangePower(nativeParameter),
                    ParameterType = paramType,
                    IsAdvanced = NativeApi.GetParameterIsAdvanced(nativeParameter),
                    CanSyncToHostBPM = NativeApi.GetParameterCanSyncToHostBPM(nativeParameter),
                    HostBPMSyncNumerator = NativeApi.GetParameterBPMSyncNumerator(nativeParameter),
                    HostBPMSyncDenominator = NativeApi.GetParameterBPMSyncDenominator(nativeParameter),
                    ValueFormat = Marshal.PtrToStringAnsi(NativeApi.GetParameterDisplayFormat(nativeParameter)),
                    FilePath = filePath,
                    EnumValues = enumValues,
                    IntValues = intValues,
                    GetValue = delegate
                    {
                        return NativeApi.GetParameterValue(nativeParameter);
                    },
                };

                if (parameter.CanSyncToHostBPM)
                {
                    parameter.UpdateBPMSync = delegate
                    {
                        NativeApi.SetParameterBPMSyncNumerator(nativeParameter, parameter.HostBPMSyncNumerator);
                        NativeApi.SetParameterBPMSyncDenominator(nativeParameter, parameter.HostBPMSyncDenominator);
                    };
                }

                if ((paramType == EParameterType.Enum) || (paramType == EParameterType.File))
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
    }
}
