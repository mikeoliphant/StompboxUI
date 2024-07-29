using System;
using System.Timers;
using UILayout;
using Stompbox;

namespace Stompbox
{
    public class AudioFileRecorderInterface : PluginInterface
    {
        TextBlock recordTimeText;
        Timer recordTimer;
        DateTime startTime;

        public AudioFileRecorderInterface(IAudioPlugin plugin)
            : base(plugin)
        {
            recordTimer = new Timer(1000);
            recordTimer.Elapsed += RecordTimer_Elapsed;
            recordTimer.Start();

            startTime = DateTime.Now;
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
                ImageColor = UIColor.Red,
                PressAction = delegate
                {                    
                    StompboxClient.Instance.SendCommand("SendCommand " + Plugin.ID + " Save");

                    startTime = DateTime.Now;
                }
            });
        }

        private void RecordTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int recordSecondsElapsed = (int)(e.SignalTime - startTime).TotalSeconds;

            recordTimeText.Text = (recordSecondsElapsed / 60).ToString("00") + ":" + (recordSecondsElapsed % 60).ToString("00");

            UpdateContentLayout();
        }
    }
}