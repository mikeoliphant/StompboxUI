#pragma once

#if _WIN32

#include <vcclr.h>
#include <algorithm>
#include "PluginProcessor.h"
#include "StompBox.h"

#pragma make_public(PluginProcessor)

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace UnmanagedPlugins
{
	public ref class PluginWrapper
	{
	public:
		PluginWrapper();
		~PluginWrapper()
		{
			if (component != nullptr)
				delete component;
		}

		void Initialize(double sampleRate)
		{	
			component->init(sampleRate);
		}

		System::String^ GetName()
		{
			return gcnew System::String(component->Name.c_str());
		}

		System::String^ GetID()
		{
			return gcnew System::String(component->ID.c_str());
		}

		void SetComponent(StompBox* comp)
		{
			component = comp;
		}

		System::String^ GetDescription()
		{
			return gcnew System::String(component->Description.c_str());
		}


		bool GetEnabled()
		{
			return component->Enabled;
		}

		void SetEnabled(bool enabled)
		{
			component->Enabled = enabled;
		}

		System::String^ GetBackgroundColor()
		{
			return gcnew System::String(component->BackgroundColor.c_str());
		}

		System::String^ GetForegroundColor()
		{
			return gcnew System::String(component->ForegroundColor.c_str());
		}

		bool GetIsUserSelectable()
		{
			return component->IsUserSelectable;
		}

		List<IntPtr>^ GetParameters()
		{
			List<IntPtr>^ parameters = gcnew List<IntPtr>();

			if (component->InputGain != nullptr)
			{
				parameters->Add((IntPtr) & (component->InputGain->Parameters[0]));
			}

			for (int i = 0; i < component->NumParameters; i++)
			{
				parameters->Add((IntPtr)(&(component->Parameters[i])));
			}

			if (component->OutputVolume != nullptr)
			{
				parameters->Add((IntPtr) & (component->OutputVolume->Parameters[0]));
			}

			return parameters;
		}

		double GetParameter(IntPtr parameter)
		{
			return ((StompBoxParameter*)(void*)parameter)->GetValue();
		}

		void SetParameter(IntPtr parameter, double value)
		{
			return ((StompBoxParameter*)(void*)parameter)->SetValue(value);
		}

		System::String^ GetParameterName(IntPtr parameter)
		{
			return gcnew System::String(((StompBoxParameter*)(void*)parameter)->Name.c_str());
		}

		System::String^ GetParameterDescription(IntPtr parameter)
		{
			return gcnew System::String(((StompBoxParameter*)(void*)parameter)->Description.c_str());
		}

		double GetParameterMinValue(IntPtr parameter)
		{
			return ((StompBoxParameter*)(void*)parameter)->MinValue;
		}

		double GetParameterMaxValue(IntPtr parameter)
		{
			return ((StompBoxParameter*)(void*)parameter)->MaxValue;
		}

		double GetParameterDefaultValue(IntPtr parameter)
		{
			return ((StompBoxParameter*)(void*)parameter)->DefaultValue;
		}

		double GetParameterRangePower(IntPtr parameter)
		{
			return ((StompBoxParameter*)(void*)parameter)->RangePower;
		}

		int GetParameterType(IntPtr parameter)
		{
			return ((StompBoxParameter*)(void*)parameter)->ParameterType;
		}

		bool GetParameterCanSyncToHostBPM(IntPtr parameter)
		{
			return ((StompBoxParameter*)(void*)parameter)->CanSyncToHostBPM;
		}

		int GetParameterBPMSyncNumerator(IntPtr parameter)
		{
			return ((StompBoxParameter*)(void*)parameter)->BPMSyncNumerator;
		}

		int GetParameterBPMSyncDenominator(IntPtr parameter)
		{
			return ((StompBoxParameter*)(void*)parameter)->BPMSyncDenominator;
		}

		void SetParameterBPMSyncNumerator(IntPtr parameter, int numerator)
		{
			((StompBoxParameter*)(void*)parameter)->BPMSyncNumerator = numerator;
			component->UpdateBPM();
		}

		void SetParameterBPMSyncDenominator(IntPtr parameter, int denom)
		{
			((StompBoxParameter*)(void*)parameter)->BPMSyncDenominator = denom;
			component->UpdateBPM();
		}

		bool GetParameterIsAdvanced(IntPtr parameter)
		{
			return ((StompBoxParameter*)(void*)parameter)->IsAdvanced;
		}

		System::String^ GetParameterFilePath(IntPtr parameter)
		{
			return gcnew System::String(((StompBoxParameter*)(void*)parameter)->FilePath.c_str());
		}

		array<System::String^>^ GetParameterEnumValues(IntPtr parameter)
		{
			StompBoxParameter* param = ((StompBoxParameter*)(void*)parameter);

			if (param->EnumValues == nullptr)
				return nullptr;

			array<System::String^>^ presetNames = gcnew array<System::String^>(param->EnumValues->size());

			for (int i = 0; i < param->EnumValues->size(); i++)
			{
				presetNames[i] = gcnew System::String((*param->EnumValues)[i].c_str());
			}

			return presetNames;
		}

		System::String^ GetParameterDisplayFormat(IntPtr parameter)
		{
			return gcnew System::String(((StompBoxParameter*)(void*)parameter)->DisplayFormat);
		}


		double GetOutputValue()
		{
			if (component->OutputValue != nullptr)
			{
				return *(component->OutputValue);
			}

			return 0;
		}

		void Process(double** inputs, double** outputs, unsigned int bufferSize)
		{
			component->compute(bufferSize, inputs[0], outputs[0]);
		}

		StompBox* GetComponent()
		{
			return component;
		}
	protected:
		StompBox* component = nullptr;
	};

	public delegate void MonitorCallback(double* sampleData, int numSamples);
	public delegate void MidiCallback(int midiCommand, int midiData1, int midiData2);

	public ref class PluginProcessorWrapper
	{
	public:
		PluginProcessor* processor = nullptr;

	protected:
		GCHandle monitorCallbackHandle;
		GCHandle midiCallbackHandle;

	public:
		PluginProcessorWrapper(System::String^ dataPath, bool dawMode)
		{
			std::filesystem::path path;

			char* pathChars = (char*)(void*)Marshal::StringToHGlobalAnsi(dataPath);

			path.assign(pathChars);

			Marshal::FreeHGlobal((IntPtr)pathChars);

			processor = new PluginProcessor(path, dawMode);
		}

		PluginProcessorWrapper(PluginProcessor* processor)
		{
			this->processor = processor;
		}

		~PluginProcessorWrapper()
		{
			delete processor;
		}

		void Init(double sampleRate)
		{
			processor->Init(sampleRate);
		}

		void StartServer()
		{
			processor->StartServer();
		}

		void SetBPM(double bpm)
		{
			processor->SetBPM(bpm);
		}

		bool IsPresetLoading()
		{
			return processor->IsPresetLoading();
		}

		System::String^ GetDataPath()
		{
			return gcnew System::String(processor->GetDataPath().c_str());
		}

		System::String^ DumpProgram()
		{
			return gcnew System::String(processor->DumpProgram().c_str());
		}

		System::String^ DumpSettings()
		{
			return gcnew System::String(processor->DumpSettings().c_str());
		}

		System::String^ DumpVersion()
		{
			return gcnew System::String(processor->GetVersion().c_str());
		}

		void HandleCommand(System::String^ cmd)
		{
			char* cmdChars = (char*)(void*)Marshal::StringToHGlobalAnsi(cmd);

			processor->HandleCommand(cmdChars);

			Marshal::FreeHGlobal((IntPtr)cmdChars);
		}

		bool HandleMidiCommand(int midiCommand, int midiData1, int midiData2)
		{
			return processor->HandleMidiCommand(midiCommand, midiData1, midiData2);
		}

		array<System::String^>^ GetAllPlugins()
		{
			auto pluginList = processor->GetPluginFactory()->GetAllPlugins();

			array<System::String^>^ plugins = gcnew array<System::String^>(pluginList.size());

			int pos = 0;

			for (const auto& plugin : pluginList)
			{
				plugins[pos++] = gcnew System::String(plugin.c_str());
			}

			return plugins;
		}

		System::String^ GetPluginSlot(System::String^ slotName)
		{
			char* pluginChars = (char*)(void*)Marshal::StringToHGlobalAnsi(slotName);

			StompBox* plugin = processor->GetPluginSlot(pluginChars);

			Marshal::FreeHGlobal((IntPtr)pluginChars);

			if (plugin == nullptr)
				return nullptr;

			return gcnew System::String(plugin->ID.c_str());
		}

		array<System::String^>^ GetInputPlugins()
		{
			std::list<StompBox*> chain = processor->GetInputChain();

			array<System::String^>^ plugins = gcnew array<System::String^>(chain.size());

			int pos = 0;

			for (const auto& plugin : chain)
			{
				plugins[pos++] = gcnew System::String(plugin->ID.c_str());
			}

			return plugins;
		}

		array<System::String^>^ GetFxLoopPlugins()
		{
			std::list<StompBox*> chain = processor->GetFxLoop();

			array<System::String^>^ plugins = gcnew array<System::String^>(chain.size());

			int pos = 0;

			for (const auto& plugin : chain)
			{
				plugins[pos++] = gcnew System::String(plugin->ID.c_str());
			}

			return plugins;
		}

		array<System::String^>^ GetOutputPlugins()
		{
			std::list<StompBox*> chain = processor->GetOutputChain();

			array<System::String^>^ plugins = gcnew array<System::String^>(chain.size());

			int pos = 0;

			for (const auto& plugin : chain)
			{
				plugins[pos++] = gcnew System::String(plugin->ID.c_str());
			}

			return plugins;
		}

		String^ GetPresets()
		{
			return gcnew System::String(processor->GetPresets().c_str());
		}

		String^ GetCurrentPreset()
		{
			return gcnew System::String(processor->GetCurrentPreset().c_str());
		}

		PluginWrapper^ CreatePlugin(System::String^ id)
		{
			char* idChars = (char*)(void*)Marshal::StringToHGlobalAnsi(id);

			StompBox* component = processor->CreatePlugin(idChars);

			Marshal::FreeHGlobal((IntPtr)idChars);

			if (component == nullptr)
				return nullptr;

			PluginWrapper^ wrapper = gcnew PluginWrapper();
			wrapper->SetComponent(component);

			return wrapper;
		}

		void MonitorPlugin(System::String^ id, MonitorCallback^ callback)
		{
			char* idChars = (char*)(void*)Marshal::StringToHGlobalAnsi(id);

			monitorCallbackHandle = GCHandle::Alloc(callback);

			IntPtr monitorFunctionPointer = System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(callback);
			auto funcPtr = static_cast<void(*)(double*, int)>(monitorFunctionPointer.ToPointer());

			processor->SetMonitorPlugin(idChars, funcPtr);

			Marshal::FreeHGlobal((IntPtr)idChars);
		}

		void SetMidiCallback(MidiCallback^ callback)
		{
			monitorCallbackHandle = GCHandle::Alloc(callback);

			IntPtr monitorFunctionPointer = System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(callback);
			auto funcPtr = static_cast<void(*)(int, int, int)>(monitorFunctionPointer.ToPointer());

			processor->SetMidiCallback(funcPtr);
		}

		void Process(double* input, double* output, unsigned int bufferSize)
		{
			processor->Process(input, output, bufferSize);
		}
	};

}

#endif