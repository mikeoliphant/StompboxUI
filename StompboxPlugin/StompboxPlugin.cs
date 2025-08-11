using System;
using System.Diagnostics;
using System.Threading;
using AudioPlugSharp;
using UILayout;

namespace Stompbox
{
	public unsafe class StompboxPlugin : AudioPlugSharp.AudioPluginBase
	{
		public int CurrentProgram { get; private set; }

		public StompboxAPI.APIClient StompboxClient { get; private set; }
        public MonoGameHost GameHost { get; private set; } = null;

        FloatAudioIOPort monoInput;
        FloatAudioIOPort monoOutput;
		IntPtr bitConvertBuffer = IntPtr.Zero;
		uint bitConvertBufferSize = 0;

        public StompboxPlugin()
		{
			Company = "Nostatic Software";
			Website = "nostatic.org";
			Contact = "contact@nostatic.org";
			PluginName = "stompbox";
			PluginCategory = "Fx";
			PluginVersion = "0.1.14";

			PluginID = 0x43D53D93648B49CA;

			HasUserInterface = true;

			//Logger.ImmediateMode = true;

            //Logger.ImmediateMode = true;
            //Logger.WriteToStdErr = true;
        }

        public override void Initialize()
		{
			base.Initialize();

			Debug("Initialize");

			InputPorts = new AudioIOPort[] { monoInput = new FloatAudioIOPort("Mono Input", EAudioChannelConfiguration.Mono) };
			OutputPorts = new AudioIOPort[] { monoOutput = new FloatAudioIOPort("Mono Output", EAudioChannelConfiguration.Mono) };

            EditorWidth = 1000;
            EditorHeight = 540;

            //GCSettings.LatencyMode = GCLatencyMode.LowLatency;// GCLatencyMode.Batch; // SustainedLowLatency;

            StompboxAPI.APIClient.DebugAction = Debug;

			StompboxClient = new StompboxAPI.APIClient();

            StompboxClient.MidiCallback = SendMidiCommand;

            StompboxClient.SendCommand("SetGlobalChain MasterChain MasterIn Chain Input Slot Amp Slot Tonestack Chain FxLoop Slot Cabinet Chain Output MasterChain MasterOut");

            StompboxClient.UpdateProgram();

            StompboxClient.SendCommand("SetChain MasterIn Tuner Input");
            StompboxClient.SendCommand("SetChain MasterOut AudioFilePlayer Master");

            StompboxClient.SetDefaultSlotPlugin("Amp", "NAM");
            StompboxClient.SetDefaultSlotPlugin("Tonestack", "EQ-7");
            StompboxClient.SetDefaultSlotPlugin("Cabinet", "Cabinet");
		}

        public override void Stop()
		{
			base.Stop();

			GC.Collect();
		}

		public void DisposeDevice()
		{
		}

		public void Debug(String debugStr)
		{
			Logger.Log(debugStr);
		}

		public virtual void UpdateUI()
		{
			StompboxClient.UpdateUI();
		}

		IntPtr parentWindow;

		public override void ShowEditor(IntPtr parentWindow)
		{
			Logger.Log("Show Editor");

			this.parentWindow = parentWindow;

			if (parentWindow == IntPtr.Zero)
			{
				RunGame();
			}
			else
			{
				new Thread(new ThreadStart(RunGame)).Start();
			}
		}

		void RunGame()
		{
			Logger.Log("Start game");

			try
			{				
				int screenWidth = (int)EditorWidth;
				int screenHeight = (int)EditorHeight;

                StompboxLayout game = new StompboxLayout();

				game.Scale = 0.35f;

                using (GameHost = new MonoGameHost(parentWindow, screenWidth, screenHeight, fullscreen: false))
                {
                    GameHost.IsMouseVisible = true;

                    GameHost.StartGame(game);

                    StompboxClient.NeedUIReload = true;
				}

				game = null;
            }
			catch (Exception ex)
			{
				Logger.Log("Run game failed with: " + ex.ToString());
			}
		}

		public override void ResizeEditor(uint newWidth, uint newHeight)
		{
			base.ResizeEditor(newWidth, newHeight);

			if (GameHost != null)
			{
                GameHost.RequestResize((int)newWidth, (int)newHeight);
			}
		}


        public override void HideEditor()
        {
            base.HideEditor();

            GameHost.Exit();
        }

        public override void SetMaxAudioBufferSize(uint maxSamples, EAudioBitsPerSample bitsPerSample)
        {
            base.SetMaxAudioBufferSize(maxSamples, bitsPerSample);

			StompboxClient.SetMaxAudioBufferSize(maxSamples);
        }

        public override void InitializeProcessing()
		{
			base.InitializeProcessing();

			Logger.Log("Sample rate is: " + Host.SampleRate);

			StompboxClient.Init((float)Host.SampleRate);
		}

		public override byte[] SaveState()
		{
			byte[] data = null;

			try
			{
				data = System.Text.Encoding.ASCII.GetBytes(StompboxClient.GetProgramState());
			}
			catch (Exception ex)
			{
				Logger.Log("Save state failed with: " + ex.ToString());
			}

			return data;
		}

		string[] lineSeparator = new string[] { "\r", "\n" };

		public override void RestoreState(byte[] stateData)
		{
			String programString = System.Text.Encoding.ASCII.GetString(stateData);

			string[] commands = programString.Split(lineSeparator, 0);

			foreach (string command in commands)
			{
				if (!string.IsNullOrWhiteSpace(command))
				{
					StompboxClient.SendCommand(command);
				}
			}

            UpdateUI();
		}

		Stopwatch processWatch = new Stopwatch();

		void SendMidiCommand(int midiCommand, int midiData1, int midiData2)
        {
			Logger.Log("Send midi: " + midiCommand + " " + midiData1 + " " + midiData2);

            switch (midiCommand)
            {
				case 0x80:
					Host.SendNoteOff(1, midiData1, (float)midiData2 / 127.0f, 0);
					break;
				case 0x90:
					Host.SendNoteOn(1, midiData1, (float)midiData2 / 127.0f, 0);
					break;
            }
        }

		public override void Process()
		{
			StompboxClient.Process(((float**)monoInput.GetAudioBufferPtrs())[0], ((float**)monoOutput.GetAudioBufferPtrs())[0], monoInput.CurrentBufferSize);
		}
	}
}
