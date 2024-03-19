using System;
using System.Drawing;
using System.Collections.Generic;
using System.Numerics;
using UILayout;
using Stompbox;
using System.Dynamic;

namespace Stompbox
{
    struct TargetNote
    {
        public ENoteName Note;
        public int Octave;
        public double Frequency;

        public TargetNote(ENoteName note, int octave)
        {
            this.Note = note;
            this.Octave = octave;

            this.Frequency = NoteUtil.GetMidiNoteFrequency(NoteUtil.GetMidiNoteNumber(note, octave - 1));
        }
    }

    public class TunerInterface : PluginInterface
    {
        static string[] noteNames = new string[]
        {
            "C","C#","D","Eb","E","F","F#","G","Ab","A", "Bb","B"
        };

        int tunerImageWidth = 100;
        int tunerImageHeight;
        double currentPitchCenter;
        TargetNote closestNote = new TargetNote(ENoteName.E, 2);
        double lastPitchCenter = 0;

        int queueSize = StompboxClient.Instance.InClientMode ? 20 : 40;
        Queue<double> pitchHistory = new Queue<double>();
        int frameCount = 0;
        DateTime startTime = DateTime.MinValue;

        TargetNote[] targetNotes;

        HorizontalStack noteDisplay;
        StringBuilderTextBlock tunerFrequencyDisplay;
        StringBuilderTextBlock tunerCentsDisplay;
        StringBuilderTextBlock tunerNoteDisplay;
        UIColor lineColor;
        EditableImage tunerImage;
        ImageElement tunerImageElement;

        Action<Point> lineDrawAction;
        Action<Point> tunerPointDrawAction;

        public TunerInterface(IAudioPlugin plugin)
            : base(plugin)
        {
            lineDrawAction = delegate (Point p)
            {
                tunerImage.SetPixel(p.X, p.Y, lineColor);
            };

            tunerPointDrawAction = delegate (Point p)
            {
                tunerImage.DrawCircle(p.X, p.Y, 1, UIColor.Yellow, fill: true);
            };

            plugin.SetOutputValue = UpdateTuner;

            int startNote = NoteUtil.GetMidiNoteNumber(ENoteName.B, 0);
            int endNote = NoteUtil.GetMidiNoteNumber(ENoteName.A, 6);

            targetNotes = new TargetNote[(endNote - startNote) + 1];

            int pos = 0;

            for (int n = startNote; n <= endNote; n++)
            {
                targetNotes[pos++] = new TargetNote(NoteUtil.GetNoteName(n), NoteUtil.GetNoteOctave(n));
            }

            if (!StompboxGame.DAWMode)
            {
                plugin.Enabled = true;
                StompboxClient.Instance.SendCommand("SetParam " + Plugin.ID + " Enabled " + (plugin.Enabled ? "1" : "0"));
            }
        }

        protected override void PowerButtonPressed(bool pressed)
        {
            base.PowerButtonPressed(pressed);

            if (!pressed)
            {
                tunerImage.Clear(UIColor.Black);
                tunerImage.UpdateImageData();

                tunerFrequencyDisplay.StringBuilder.Clear();
                tunerCentsDisplay.StringBuilder.Clear();
                tunerNoteDisplay.StringBuilder.Clear();
            }
        }

        protected override void AddControls(Dock dock)
        {
            lineColor = UIColor.Green;
            lineColor.A = 128;

            dock.Children.Add(new TextBlock(Plugin.Name) { HorizontalAlignment = EHorizontalAlignment.Center, TextColor = foregroundColor });

            float width = ((StompboxGame.DAWMode) ? PluginInterface.DefaultHeight : 480) * 0.8f;
            float height = ((StompboxGame.DAWMode) ? PluginInterface.DefaultHeight : 480) * 0.5f;

            VerticalStack pitchStack = new VerticalStack
            {
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
                BackgroundColor = UIColor.Black
            };
            dock.Children.Add(pitchStack);

            tunerImageHeight = (int)((height / width) * tunerImageWidth);

            tunerImage = new EditableImage(new UIImage(tunerImageWidth, tunerImageHeight));
            tunerImage.Clear(UIColor.Black);
            tunerImage.UpdateImageData();

            tunerImageElement = new ImageElement(tunerImage.Image)
            {
                DesiredWidth = 400,
                DesiredHeight = 200
            };
            tunerImageElement.Image = tunerImage.Image;
            pitchStack.Children.Add(tunerImageElement);

            noteDisplay = new HorizontalStack
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Bottom,
                Padding = new LayoutPadding(4),
                DesiredHeight = 60,
                ChildSpacing = 4,
                BackgroundColor = lineColor
            };
            pitchStack.Children.Add(noteDisplay);

            tunerFrequencyDisplay = new StringBuilderTextBlock
            {
                TextFont = Layout.Current.GetFont("SmallFont"),
                TextColor = UIColor.White,
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
            };

            noteDisplay.Children.Add(new UIElementWrapper()
            {
                VerticalAlignment = EVerticalAlignment.Stretch,
                DesiredWidth = 160,
                BackgroundColor = UIColor.Black,
                Child = tunerFrequencyDisplay
            });

            tunerNoteDisplay = new StringBuilderTextBlock
            {
                TextColor = UIColor.White,
                HorizontalAlignment = EHorizontalAlignment.Center,
                VerticalAlignment = EVerticalAlignment.Center,
            };

            noteDisplay.Children.Add(new UIElementWrapper()
            {
                HorizontalAlignment = EHorizontalAlignment.Stretch,
                VerticalAlignment = EVerticalAlignment.Stretch,
                BackgroundColor = UIColor.Black,
                Child = tunerNoteDisplay
            });

           tunerCentsDisplay = new StringBuilderTextBlock
           {
               TextFont = Layout.Current.GetFont("SmallFont"),
               TextColor = UIColor.White,
               HorizontalAlignment = EHorizontalAlignment.Center,
               VerticalAlignment = EVerticalAlignment.Center,
           };

            noteDisplay.Children.Add(new UIElementWrapper()
            {
                VerticalAlignment = EVerticalAlignment.Stretch,
                DesiredWidth = 160,
                BackgroundColor = UIColor.Black,
                Child = tunerCentsDisplay
            });

            showOptionsMenu = false;

            base.AddControls(dock);
        }

        int lastClosestNote = -1;

        void UpdateTuner(double value)
        {
            if (frameCount == 0)
            {
                DateTime now = DateTime.Now;

                if (startTime != DateTime.MinValue)
                {
                    double elapsedSecs = (now - startTime).TotalSeconds;

                    if (elapsedSecs < 1)
                    {
                        queueSize = (int)((1 / elapsedSecs) * 10);
                    }
                }

                startTime = now;
                frameCount = 10;
            }
            else
            {
                frameCount--;
            }

            tunerImage.Clear(UIColor.Black);

            double newPitch = value;

            pitchHistory.Enqueue(newPitch);

            while (pitchHistory.Count > queueSize)
                pitchHistory.Dequeue();

            if (newPitch > 0)
            {
                double diff = double.MaxValue;

                foreach (TargetNote note in targetNotes)
                {
                    double targetDiff = Math.Abs(note.Frequency - newPitch);

                    if (targetDiff > diff)
                        break;

                    diff = targetDiff;

                    closestNote = note;
                }

                //Logging.Log("Pitch: " + newPitch + " Closest: " + closestNote.Note.ToString() + closestNote.Octave.ToString() + " " + closestNote.Frequency);

                currentPitchCenter = closestNote.Frequency;

                if (currentPitchCenter != lastPitchCenter)
                {
                    lastPitchCenter = currentPitchCenter;
                }

                int integer = (int)Math.Floor(newPitch);

                tunerFrequencyDisplay.StringBuilder.Clear();
                tunerFrequencyDisplay.StringBuilder.AppendNumber(integer);
                tunerFrequencyDisplay.StringBuilder.Append('.');
                tunerFrequencyDisplay.StringBuilder.AppendNumber((int)Math.Round((newPitch - (float)integer) * 10));
                tunerFrequencyDisplay.StringBuilder.Append("Hz");

                int cents = (int)Math.Round(1200 * Math.Log(newPitch / currentPitchCenter, 2));

                tunerCentsDisplay.StringBuilder.Clear();

                if (cents != 0)
                    tunerCentsDisplay.StringBuilder.Append((cents > 0) ? "+" : "-");

                tunerCentsDisplay.StringBuilder.AppendNumber(Math.Abs(cents));
            }

            if (lastClosestNote != (int)closestNote.Note)
            {
                tunerNoteDisplay.StringBuilder.Clear();
                tunerNoteDisplay.StringBuilder.Append(noteNames[(int)closestNote.Note]);

                lastClosestNote = (int)closestNote.Note;
            }

            double closestY = (tunerImageHeight / 2) - 1;

            tunerImage.DrawLine(new Vector2(0, (int)closestY), new Vector2(tunerImage.ImageWidth - 1, (int)closestY), lineDrawAction);

            double step = (double)tunerImageWidth / (double)(queueSize - 1);

            double offset = (step / 2);

            double range = .03 / (currentPitchCenter / 83.0);   // Set range based on pitch (smaller range for higher frequencies)

            int lastX = -1;
            int lastY = -1;

            foreach (double pitch in pitchHistory)
            {
                if (pitch > 0)
                {
                    double y = ((double)tunerImageHeight / 2) + (((1.0 - (pitch / currentPitchCenter)) / range) * ((double)tunerImageHeight / 2));

                    double xOffset = offset;
                    double yOffset = y;

                    if ((yOffset > 0) && (yOffset < tunerImageHeight))
                    {
                        if (lastX != -1)
                        {
                            tunerImage.DrawLine(new Vector2(lastX, lastY), new Vector2((int)xOffset, (int)yOffset), tunerPointDrawAction);
                        }

                        lastX = (int)xOffset;
                        lastY = (int)yOffset;
                    }
                }

                offset += step;
            }

            tunerImage.UpdateImageData();

            noteDisplay.UpdateContentLayout();
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            if (Plugin.Enabled && !Plugin.StompboxClient.InClientMode)
            {
                UpdateTuner(Plugin.OutputValue);
            }
        }

        //public override void Update(float secondsElapsed)
        //{
        //    base.Update(secondsElapsed);

        //    if (Plugin.Enabled && !Plugin.StompboxClient.InClientMode)
        //    {
        //        UpdateTuner(Plugin.OutputValue);
        //    }
        //}

        public override void Close()
        {
            Plugin.Enabled = false;
            StompboxClient.Instance.SendCommand("SetParam " + Plugin.ID + " Enabled " + (Plugin.Enabled ? "1" : "0"));

            base.Close();
        }
    }
}
