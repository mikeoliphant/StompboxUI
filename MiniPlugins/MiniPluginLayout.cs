using System;
using UILayout;
using Stompbox;

namespace MiniPlugins
{
    public class MiniPluginLayout : MonoGameLayout
    {
        public static MiniPluginLayout Instance { get; private set; }
        public static float BaseScale { get; set; } = 1.0f;

        bool clientConnected = true;

        public MiniPluginLayout()
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

            RootUIElement = new MiniPluginInterface();

            Initialize();
        }

        void Initialize()
        {
            StompboxClient.Instance.SendCommand("SetGlobalChain MasterSlot InputGain Slot Amp Slot Tonestack Slot Cabinet MasterSlot MasterVolume");

            StompboxClient.Instance.UpdateProgram();

            StompboxClient.Instance.SendCommand("SetSlot InputGain Input");
            StompboxClient.Instance.SendCommand("SetChain MasterVolume Master");
            StompboxClient.Instance.SendCommand("SetPluginSlot Amp NAM");
            StompboxClient.Instance.SendCommand("SetPluginSlot Tonestack EQ-7");
            StompboxClient.Instance.SendCommand("SetPluginSlot Cabinet Cabinet");
        }
    }
}