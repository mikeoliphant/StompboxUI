﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Numerics;
using UILayout;
using Stompbox;

namespace Stompbox
{
    struct TargetNote
    {
        public ENoteName Note;
        public int Octave;
        public float Frequency;

        public TargetNote(ENoteName note, int octave)
        {
            this.Note = note;
            this.Octave = octave;

            this.Frequency = (float)NoteUtil.GetMidiNoteFrequency(NoteUtil.GetMidiNoteNumber(note, octave - 1));
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
        float currentPitchCenter;
        TargetNote closestNote = new TargetNote(ENoteName.E, 2);
        float lastPitchCenter = 0;
        float runningCentsOffset = 0;

        int queueSize = StompboxClient.Instance.InClientMode ? 20 : 40;
        ConcurrentQueue<float> pitchHistory = new ConcurrentQueue<float>();
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
            : base()
        {
            ShowOutputParameters = false;

            SetPlugin(plugin);

            lineDrawAction = delegate (Point p)
            {
                tunerImage.SetPixel(p.X, p.Y, lineColor);
            };

            tunerPointDrawAction = delegate (Point p)
            {
                tunerImage.DrawCircle(p.X, p.Y, 1, UIColor.Yellow, fill: true);
            };

            int startNote = NoteUtil.GetMidiNoteNumber(ENoteName.B, 0);
            int endNote = NoteUtil.GetMidiNoteNumber(ENoteName.A, 6);

            targetNotes = new TargetNote[(endNote - startNote) + 1];

            int pos = 0;

            for (int n = startNote; n <= endNote; n++)
            {
                targetNotes[pos++] = new TargetNote(NoteUtil.GetNoteName(n), NoteUtil.GetNoteOctave(n));
            }

            if (StompboxClient.Instance.InClientMode)
            {
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

            float width = PluginInterface.DefaultHeight * 0.8f;
            float height = PluginInterface.DefaultHeight * 0.5f;

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

        void UpdateTuner(float value)
        {
            if (frameCount == 0)
            {
                DateTime now = DateTime.Now;

                if (startTime != DateTime.MinValue)
                {
                    float elapsedSecs = (float)(now - startTime).TotalSeconds;

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

            float newPitch = value;

            pitchHistory.Enqueue(newPitch);

            while (pitchHistory.Count > queueSize)
            {
                float val;

                pitchHistory.TryDequeue(out val);
            }

            if (newPitch > 20)
            {
                float diff = float.MaxValue;

                foreach (TargetNote note in targetNotes)
                {
                    float targetDiff = Math.Abs(note.Frequency - newPitch);

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

                float centsOffset = (float)(1200 * Math.Log(newPitch / currentPitchCenter, 2));

                if (Math.Abs(centsOffset - runningCentsOffset) > 10)
                {
                    runningCentsOffset = centsOffset;
                }
                else
                {
                    runningCentsOffset = (runningCentsOffset * .99f) + (centsOffset * .01f);
                }

                tunerCentsDisplay.StringBuilder.Clear();

                if (runningCentsOffset != 0)
                    tunerCentsDisplay.StringBuilder.Append((runningCentsOffset > 0) ? "+" : "-");

                tunerCentsDisplay.StringBuilder.AppendNumber(Math.Abs((int)Math.Round(runningCentsOffset)));
            }

            if (lastClosestNote != (int)closestNote.Note)
            {
                tunerNoteDisplay.StringBuilder.Clear();
                tunerNoteDisplay.StringBuilder.Append(noteNames[(int)closestNote.Note]);

                lastClosestNote = (int)closestNote.Note;
            }

            float closestY = (tunerImageHeight / 2) - 1;

            float step = (float)tunerImageWidth / (float)(queueSize - 1);

            float offset = (step / 2);

            int lastX = -1;
            int lastY = -1;

            foreach (float pitch in pitchHistory)
            {
                if (pitch > 0)
                {
                    float semitoneOffset = (float)(12 * Math.Log(pitch / currentPitchCenter, 2));

                    float y = ((float)tunerImageHeight / 2) + (-semitoneOffset  * (float)tunerImageHeight);

                    float xOffset = offset;
                    float yOffset = y;

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

            tunerImage.DrawLine(new Vector2(0, (int)closestY), new Vector2(tunerImage.ImageWidth - 1, (int)closestY), lineDrawAction);

            tunerImage.UpdateImageData();

            noteDisplay.UpdateContentLayout();
        }

        protected override void DrawContents()
        {
            base.DrawContents();

            if (Plugin.Enabled)
            {
                UpdateTuner(Plugin.GetParameter("Pitch").Value);
            }
        }

        public override void Close()
        {
            Plugin.Enabled = false;

            base.Close();
        }
    }
}
