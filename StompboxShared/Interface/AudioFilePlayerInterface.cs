using System;
using UILayout;
using Stompbox;

namespace Stompbox
{
    public class AudioFilePlayerInterface : PluginInterface
    {
        public AudioFilePlayerInterface(IAudioPlugin plugin)
            : base(plugin)
        {

        }

        protected override void AddControls(Dock dock)
        {
            VerticalStack vStack = new VerticalStack { HorizontalAlignment = EHorizontalAlignment.Stretch, VerticalAlignment = EVerticalAlignment.Stretch, ChildSpacing = 5 };
            dock.Children.Add(vStack);

            vStack.Children.Add(new TextBlock(Plugin.Name) { HorizontalAlignment = EHorizontalAlignment.Center, TextColor = foregroundColor });

            vStack.Children.Add(CreateEnumControl(Plugin.Parameters[0]));

            UIElement levelControl = CreateControl(Plugin.Parameters[1]);
            levelControl.HorizontalAlignment = EHorizontalAlignment.Center;

            vStack.Children.Add(levelControl);

            HorizontalStack controlStack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Center, ChildSpacing = 5 };
            vStack.Children.Add(controlStack);

            ImageToggleButton playButton = new ImageToggleButton("Stop", "Play")
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
            };

            playButton.PressAction = delegate
            {
                Plugin.Enabled = playButton.IsPressed;
                Plugin.GetParameter("Playing").Value = playButton.IsPressed ? 1 : 0;
            };

            playButton.SetPressed(Plugin.GetParameter("Playing").Value == 1);

            playButton.ImageColor = foregroundColor;

            controlStack.Children.Add(playButton);

            controlStack.Children.Add(new ImageButton("Restart")
            {
                ImageColor = foregroundColor,
                PressAction = delegate
                {
                    StompboxClient.Instance.SendCommand("SetParam " + Plugin.ID + " Position 0");
                }
            });

            controlStack.Children.Add(new ImageButton("Record")
            {
                ImageColor = UIColor.Red,
                PressAction = delegate
                {
                    StompboxClient.Instance.SendCommand("SendCommand " + Plugin.ID + " ArmRecord");
                }
            });
        }
    }
}