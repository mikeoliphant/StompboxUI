using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Stompbox
{
    public enum ENoteName
    {
        C,
        [Description("Cs/Df")]
        CsDf,
        D,
        [Description("Ds/Ef")]
        DsEf,
        E,
        F,
        [Description("Fs/Gf")]
        FsGf,
        G,
        [Description("Gs/Af")]
        GsAf,
        A,
        [Description("As/Bf")]
        AsBf,
        B
    }

    public class NoteUtil
    {
        private static Dictionary<EChordType, int[]> scales = new Dictionary<EChordType, int[]>();
        private static double HalfStepRatio = Math.Pow(2.0, (1.0 / 12.0));
        private static double A4Frequency = 440.0;
        private static int A4MidiNoteNum = 57;

        public static Dictionary<EChordType, int[]> Scales
        {
            get { return NoteUtil.scales; }
            set { NoteUtil.scales = value; }
        }

        static NoteUtil()
        {
            scales[EChordType.Maj] = new int[] { 0, 2, 4, 5, 7, 9, 11 };
            scales[EChordType.Min] = new int[] { 0, 2, 3, 5, 7, 8, 11 };
        }

        public static bool TryParseNoteFrequency(string noteStr, out double frequency)
        {
            ENoteName note;
            int octave;

            frequency = 0;

            if (TryParseNoteName(noteStr, out note, out octave))
            {
                frequency = GetMidiNoteFrequency(GetMidiNoteNumber(note, octave));

                return true;
            }

            return false;
        }

        public static bool TryParseNoteName(string noteStr, out ENoteName note, out int octave)
        {
            note = ENoteName.C;
            octave = 0;

            string lowerStr = noteStr.ToLower().Trim();

            if (lowerStr.StartsWith("cs") || lowerStr.StartsWith("df"))
            {
                note = ENoteName.CsDf;

                return int.TryParse(lowerStr.Substring(2, 1), out octave);
            }

            if (lowerStr.StartsWith("ds") || lowerStr.StartsWith("ef"))
            {
                note = ENoteName.DsEf;

                return int.TryParse(lowerStr.Substring(2, 1), out octave);
            }

            if (lowerStr.StartsWith("fs") || lowerStr.StartsWith("gf"))
            {
                note = ENoteName.FsGf;

                return int.TryParse(lowerStr.Substring(2, 1), out octave);
            }

            if (lowerStr.StartsWith("gs") || lowerStr.StartsWith("af"))
            {
                note = ENoteName.GsAf;

                return int.TryParse(lowerStr.Substring(2, 1), out octave);
            }

            if (lowerStr.StartsWith("as") || lowerStr.StartsWith("bf"))
            {
                note = ENoteName.AsBf;

                return int.TryParse(lowerStr.Substring(2, 1), out octave);
            }

            if (lowerStr.StartsWith("c"))
            {
                note = ENoteName.C;

                return int.TryParse(lowerStr.Substring(1, 1), out octave);
            }

            if (lowerStr.StartsWith("d"))
            {
                note = ENoteName.D;

                return int.TryParse(lowerStr.Substring(1, 1), out octave);
            }

            if (lowerStr.StartsWith("e"))
            {
                note = ENoteName.E;

                return int.TryParse(lowerStr.Substring(1, 1), out octave);
            }

            if (lowerStr.StartsWith("f"))
            {
                note = ENoteName.F;

                return int.TryParse(lowerStr.Substring(1, 1), out octave);
            }


            if (lowerStr.StartsWith("g"))
            {
                note = ENoteName.G;

                return int.TryParse(lowerStr.Substring(1, 1), out octave);
            }

            if (lowerStr.StartsWith("a"))
            {
                note = ENoteName.A;

                return int.TryParse(lowerStr.Substring(1, 1), out octave);
            }


            if (lowerStr.StartsWith("b"))
            {
                note = ENoteName.B;

                return int.TryParse(lowerStr.Substring(1, 1), out octave);
            }

            return false;
        }

        public static double GetMidiNoteFrequency(int midiNoteNum)
        {
            int noteDiff = A4MidiNoteNum - midiNoteNum;

            return A4Frequency / Math.Pow(HalfStepRatio, noteDiff);
        }

        public static ENoteName GetNoteName(int midiNoteNum)
        {
            return (ENoteName)(midiNoteNum % 12);
        }

        public static int GetNoteOctave(int midiNoteNum)
        {
            return (midiNoteNum / 12) - 1;
        }

        public static int GetMidiNoteNumber(ENoteName note, int octave)
        {
            return ((octave + 1) * 12) + (int)note;
        }
    }

    public enum EChordType
    {
        Maj,
        Min
    }
}
