using System;
using UILayout;
using StompboxAPI;

namespace Stompbox
{
    public class StompboxLayout : MonoGameLayout
    {
        public static StompboxLayout Instance { get; private set; }
        public static float BaseScale { get; set; } = 1.0f;

        bool clientConnected = true;

        public StompboxLayout()
        {
            Instance = this;

            Scale = BaseScale;
        }

        public override void SetHost(MonoGameHost host)
        {
            base.SetHost(host);

            Host.InactiveSleepTime = TimeSpan.Zero;

            host.Window.Title = "stompbox";

            LoadImageManifest("ImageManifest.xml");

            GraphicsContext.SingleWhitePixelImage = GetImage("SingleWhitePixel");
            GraphicsContext.SamplerState = new Microsoft.Xna.Framework.Graphics.SamplerState()
            {
                AddressU = Microsoft.Xna.Framework.Graphics.TextureAddressMode.Clamp,
                AddressV = Microsoft.Xna.Framework.Graphics.TextureAddressMode.Clamp,
                Filter = Microsoft.Xna.Framework.Graphics.TextureFilter.Anisotropic,
                MipMapLevelOfDetailBias = -0.8f
            };

            DefaultFont = GetFont("MainFont");
            DefaultFont.SpriteFont.Spacing = 1;

            GetFont("SmallFont").SpriteFont.Spacing = 0;

            DefaultForegroundColor = UIColor.White;

            DefaultOutlineNinePatch = GetImage("PopupBackground");

            DefaultPressedNinePatch = GetImage("ButtonPressed");
            DefaultUnpressedNinePatch = GetImage("ButtonUnpressed");

            DefaultDragImage = GetImage("ButtonPressed");

            InputManager.AddMapping("StompMode", new KeyMapping(InputKey.D1));
            InputManager.AddMapping("Stomp1", new KeyMapping(InputKey.D2));
            InputManager.AddMapping("Stomp2", new KeyMapping(InputKey.D3));
            InputManager.AddMapping("Stomp3", new KeyMapping(InputKey.D4));

            InterfaceBase.SetInterfaceType(InterfaceBase.InterfaceType);

            if (!StompboxClient.Instance.InClientMode)
            {
                Initialize();
            }
        }

        public void Connect()
        {
            clientConnected = false;

            string serverName = "raspberrypi";

            StompboxClient.Instance.Connect(serverName, 24639, ConnectCallback);
        }

        void ConnectCallback(bool connected)
        {
            if (!connected)
            {
                Layout.Current.ShowTextInputPopup("Enter server:", "raspberrypi", delegate (string serverName)
                {
                    try
                    {
                        StompboxClient.Instance.Connect(serverName, 24639, ConnectCallback);
                    }
                    catch { }
                });
            }
            else
            {
                clientConnected = true;

                Initialize();
            }
        }

        void Initialize()
        {
            StompboxClient.Instance.SendCommand("SetGlobalChain MasterChain MasterIn Chain Input Slot Amp Slot Tonestack Chain FxLoop Slot Cabinet Chain Output MasterChain MasterOut");

            StompboxClient.Instance.UpdateProgram();

            StompboxClient.Instance.SendCommand("SetChain MasterIn AudioRecorder Tuner Input");
            StompboxClient.Instance.SendCommand("SetChain MasterOut AudioFilePlayer Master");
            StompboxClient.Instance.SendCommand("SetPluginSlot Amp NAM");
            StompboxClient.Instance.SendCommand("SetPluginSlot Tonestack EQ-7");
            StompboxClient.Instance.SendCommand("SetPluginSlot Cabinet Cabinet");
        }

        public override void Update(float secondsElapsed)
        {
            base.Update(secondsElapsed);

            if (StompboxClient.Instance.InClientMode)
            {
                if (clientConnected && !StompboxClient.Instance.Connected)
                {
                    Connect();
                }
            }

            if (InputManager.WasClicked("StompMode", this))
            {
                StompboxClient.Instance.SendCommand("HandleMidi " + (0xb0).ToString() + " 32 127");
            }
            else if (InputManager.WasClicked("Stomp1", this))
            {
                StompboxClient.Instance.SendCommand("HandleMidi " + (0xb0).ToString() + " 33 127");
            }
            else if (InputManager.WasClicked("Stomp2", this))
            {
                StompboxClient.Instance.SendCommand("HandleMidi " + (0xb0).ToString() + " 34 127");
            }
            else if (InputManager.WasClicked("Stomp3", this))
            {
                StompboxClient.Instance.SendCommand("HandleMidi " + (0xb0).ToString() + " 35 127");
            }
        }
    }
}