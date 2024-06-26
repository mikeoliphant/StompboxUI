using System;
using Microsoft.Xna.Framework;
using UILayout;

namespace Stompbox
{
    public class StompboxGame : MonoGameLayout
    {
        public static StompboxGame Instance { get; private set; }

        public static bool DAWMode { get; set; } = true;

        public StompboxGame()
        {
            Instance = this;

            Scale = 0.35f;

            //PixGame.Debug("Guitar Game Created");

            //if (DAWMode)
            //    PixGame.Debug("Runing in DAW mode");
        }

        public override void SetHost(Game host)
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

            float padding = DAWMode ? 12 : 20;

            //TextTouchButton.DefaultTextHorizontalPadding = padding;
            //TextTouchButton.DefaultTextVerticalPadding = padding;
            //ImageTouchButton.DefaultImageHorizontalPadding = padding;
            //ImageTouchButton.DefaultImageVerticalPadding = padding;

            DefaultPressedNinePatch = GetImage("ButtonPressed");
            DefaultUnpressedNinePatch = GetImage("ButtonUnpressed");

            DefaultDragImage = GetImage("ButtonPressed");

            InputManager.AddMapping("StompMode", new KeyMapping(InputKey.D1));
            InputManager.AddMapping("Stomp1", new KeyMapping(InputKey.D2));
            InputManager.AddMapping("Stomp2", new KeyMapping(InputKey.D3));
            InputManager.AddMapping("Stomp3", new KeyMapping(InputKey.D4));

            RootUIElement = new MainInterface();
        }

        public override void Update(float secondsElapsed)
        {
            base.Update(secondsElapsed);

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