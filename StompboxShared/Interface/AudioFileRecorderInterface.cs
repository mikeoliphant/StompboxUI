using System;
using UILayout;
using Stompbox;

namespace Stompbox
{
    public class AudioFileRecorderInterface : PluginInterface
    {
        static DateTime recordStartTime;
        static int recordSecondsElapsed;
        TextBlock recordTimeText;

        static AudioFileRecorderInterface()
        {
            recordStartTime = DateTime.Now;
        }

        public static void Init()
        {

        }

        public AudioFileRecorderInterface(IAudioPlugin plugin)
            : base(plugin)
        {
        }

        protected override void AddControls(Dock dock)
        {
            VerticalStack vStack = new VerticalStack { HorizontalAlignment = EHorizontalAlignment.Stretch, VerticalAlignment = EVerticalAlignment.Stretch, ChildSpacing = 5 };
            dock.Children.Add(vStack);

            vStack.Children.Add(new TextBlock(Plugin.Name) { HorizontalAlignment = EHorizontalAlignment.Center, TextColor = foregroundColor });

            recordTimeText = new TextBlock("-----") { HorizontalAlignment = EHorizontalAlignment.Center, VerticalAlignment = EVerticalAlignment.Stretch, TextColor = foregroundColor };
            vStack.Children.Add(recordTimeText);

            HorizontalStack controlStack = new HorizontalStack { HorizontalAlignment = EHorizontalAlignment.Center, VerticalAlignment = EVerticalAlignment.Stretch, ChildSpacing = 5 };
            vStack.Children.Add(controlStack);
            
            controlStack.Children.Add(new ImageButton("Record")
            {
                VerticalAlignment = EVerticalAlignment.Center,
                //Color = UIColor.Red,
                PressAction = delegate
                {
                    recordStartTime = DateTime.Now;
                    recordSecondsElapsed = -1;
                    
                    StompboxClient.Instance.SendCommand("SendCommand " + Plugin.ID + " Save");
                }
            });

            recordSecondsElapsed = -1;

            //Update(0);
        }

        //public override void Update(float secondsElapsed)
        //{
        //    base.Update(secondsElapsed);

        //    int seconds = (int)(DateTime.Now - recordStartTime).TotalSeconds;

        //    if (recordSecondsElapsed != seconds)
        //    {
        //        recordSecondsElapsed = seconds;

        //        recordTimeText.Text = (recordSecondsElapsed / 60).ToString("00") + ":" + (recordSecondsElapsed % 60).ToString("00");

        //        UpdateContentLayout();
        //    }
        //}
    }
}