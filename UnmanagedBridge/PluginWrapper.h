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

		int GetNumParameters()
		{
			return component->NumParameters;
		}

		double GetParameter(int index)
		{
			return component->GetParameterValue(index);
		}

		void SetParameter(int index, double value)
		{
			component->SetParameterValue(index, value);
		}

		System::String^ GetParameterName(int index)
		{
			return gcnew System::String(component->Parameters[index].Name.c_str());
		}

		double GetParameterMinValue(int index)
		{
			return component->Parameters[index].MinValue;
		}

		double GetParameterMaxValue(int index)
		{
			return component->Parameters[index].MaxValue;
		}

		double GetParameterDefaultValue(int index)
		{
			return component->Parameters[index].DefaultValue;
		}

		int GetParameterType(int index)
		{
			return component->Parameters[index].ParameterType;
		}

		bool GetParameterCanSyncToHostBPM(int index)
		{
			return component->Parameters[index].CanSyncToHostBPM;
		}

		int GetParameterBPMSyncNumerator(int index)
		{
			return component->Parameters[index].BPMSyncNumerator;
		}

		int GetParameterBPMSyncDenominator(int index)
		{
			return component->Parameters[index].BPMSyncDenominator;
		}

		void SetParameterBPMSyncNumerator(int index, int numerator)
		{
			component->Parameters[index].BPMSyncNumerator = numerator;
			component->UpdateBPM();
		}

		void SetParameterBPMSyncDenominator(int index, int denom)
		{
			component->Parameters[index].BPMSyncDenominator = denom;
			component->UpdateBPM();
		}

		bool GetParameterIsAdvanced(int index)
		{
			return component->Parameters[index].IsAdvanced;
		}

		array<System::String^>^ GetParameterEnumValues(int index)
		{
			StompBoxParameter param = component->Parameters[index];

			if (param.EnumValues == nullptr)
				return nullptr;

			array<System::String^>^ presetNames = gcnew array<System::String^>(param.EnumValues->size());

			for (int i = 0; i < param.EnumValues->size(); i++)
			{
				presetNames[i] = gcnew System::String((*param.EnumValues)[i].c_str());
			}

			return presetNames;
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

		System::String^ GetParameterDisplayFormat(int index)
		{
			return gcnew System::String(component->Parameters[index].DisplayFormat);
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
			std::list<std::string> pluginList = processor->GetPluginFactory()->GetUserPlugins();

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