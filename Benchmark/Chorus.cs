/* ------------------------------------------------------------
author: "Albert Graef"
name: "Chorus Mono"
version: "1.0"
Code generated with Faust 2.30.14 (https://faust.grame.fr)
Compilation options: -lang csharp -es 1 -double -ftz 0
------------------------------------------------------------ */
/************************************************************************
    FAUST Architecture File
    Copyright (C) 2021 Mike Oliphant
    ---------------------------------------------------------------------
    This Architecture section is free software; you can redistribute it
    and/or modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 3 of
    the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; If not, see <http://www.gnu.org/licenses/>.

    EXCEPTION : As a special exception, you may create a larger work
    that contains this FAUST architecture section and distribute
    that work under terms of your choice, so long as this FAUST
    architecture section is not modified.

 ************************************************************************
 ************************************************************************/


using System;
using System.Runtime.CompilerServices;


public class Chorus : dsp, IFaustDSP
{
	
	static double[] ftbl0ChorusSIG0 = new double[65536];
	
	
	class ChorusSIG0
	{
		
		int iRec6_0; int iRec6_1;
		
		public int GetNumInputsChorusSIG0()
		{
			return 0;
		}

		public int GetNumOutputsChorusSIG0()
		{
			return 1;
		}

		public int GetInputRateChorusSIG0(int channel)
		{
			int rate;
			switch (channel)
			{
				default:
					rate = -1;
					break;
			}
			return rate;
		}

		public int GetOutputRateChorusSIG0(int channel)
		{
			int rate;
			switch (channel)
			{
				case 0:
					rate = 0;
					break;
				default:
					rate = -1;
					break;
			}
			return rate;
		}

		
		public void instanceInitChorusSIG0(int sample_rate) {
			//for (int l3 = 0; (l3 < 2); l3 = (l3 + 1)) {
			//	iRec6_l3 = 0;
			//}
			
		}
		public void fillChorusSIG0(int count, double[] table) {
			for (int i = 0; (i < count); i = (i + 1)) {
				iRec6_0 = (iRec6_1 + 1);
				table[i] = Math.Sin((9.5873799242852573e-05 * (double)((iRec6_0 + -1))));
				iRec6_1 = iRec6_0;
			}
			
		}
	};

	ChorusSIG0 newChorusSIG0() {return new ChorusSIG0(); }
	void deleteChorusSIG0(ChorusSIG0 dsp) {}
	
	double fHslider0;
	double fRec0_0; double fRec0_1;
	double fVslider0;
	int IOTA;
	double[] fVec0 = new double[131072];
	int fSampleRate;
	double fConst1;
	double fHslider1;
	double fConst2;
	double fHslider2;
	double fRec5_0; double fRec5_1;
	double fConst3;
	double fConst4;
	double fRec1_0; double fRec1_1;
	double fRec2_0; double fRec2_1;
	double fRec3_0; double fRec3_1;
	double fRec4_0; double fRec4_1;
	
	public Chorus()
	{
		SetMetaData();
		BuildUserInterface();
	}
	
	public int GetNumInputs()
	{
		return 1;
	}

	public int GetNumOutputs()
	{
		return 1;
	}

	public int GetInputRate(int channel)
	{
		int rate;
		switch (channel)
		{
			case 0:
				rate = 1;
				break;
			default:
				rate = -1;
				break;
		}
		return rate;
	}

	public int GetOutputRate(int channel)
	{
		int rate;
		switch (channel)
		{
			case 0:
				rate = 1;
				break;
			default:
				rate = -1;
				break;
		}
		return rate;
	}

	
	void SetMetaData()
	{
		MetaData.Declare("author", "Albert Graef");
		MetaData.Declare("basics.lib/name", "Faust Basic Element Library");
		MetaData.Declare("basics.lib/version", "0.1");
		MetaData.Declare("category", "Modulation");
		MetaData.Declare("chorus.dsp/author", "Albert Graef");
		MetaData.Declare("chorus.dsp/category", "Modulation");
		MetaData.Declare("chorus.dsp/name", "Chorus");
		MetaData.Declare("chorus.dsp/version", "1.0");
		MetaData.Declare("compile_options", "-lang csharp -es 1 -double -ftz 0");
		MetaData.Declare("delays.lib/name", "Faust Delay Library");
		MetaData.Declare("delays.lib/version", "0.1");
		MetaData.Declare("filename", "chorus_mono.dsp");
		MetaData.Declare("maths.lib/author", "GRAME");
		MetaData.Declare("maths.lib/copyright", "GRAME");
		MetaData.Declare("maths.lib/license", "LGPL with exception");
		MetaData.Declare("maths.lib/name", "Faust Math Library");
		MetaData.Declare("maths.lib/version", "2.3");
		MetaData.Declare("name", "Chorus Mono");
		MetaData.Declare("platform.lib/name", "Generic Platform Library");
		MetaData.Declare("platform.lib/version", "0.1");
		MetaData.Declare("signals.lib/name", "Faust Signal Routing Library");
		MetaData.Declare("signals.lib/version", "0.0");
		MetaData.Declare("version", "1.0");
	}

	
	void BuildUserInterface()
	{
		UIDefinition.StartBox(new FaustBoxElement(EFaustUIElementType.VerticalBox, "Chorus Mono"));
		UIDefinition.AddElement(new FaustUIWriteableFloatElement(EFaustUIElementType.HorizontalSlider, "depth", new FaustVariableAccessor {
				ID = "fHslider1",
				SetValue = delegate(double val) { fHslider1 = val; },
				GetValue = delegate { return fHslider1; }
			}
			, 0.02, 0.0, 1.0, 0.01));
		UIDefinition.AddElement(new FaustUIWriteableFloatElement(EFaustUIElementType.HorizontalSlider, "freq", new FaustVariableAccessor {
				ID = "fHslider2",
				SetValue = delegate(double val) { fHslider2 = val; },
				GetValue = delegate { return fHslider2; }
			}
			, 2.0, 0.0, 10.0, 0.01));
		UIDefinition.AddElement(new FaustUIWriteableFloatElement(EFaustUIElementType.HorizontalSlider, "level", new FaustVariableAccessor {
				ID = "fHslider0",
				SetValue = delegate(double val) { fHslider0 = val; },
				GetValue = delegate { return fHslider0; }
			}
			, 0.5, 0.0, 1.0, 0.01));
		UIDefinition.DeclareElementMetaData("fVslider0", "name", "wet/dry");
		UIDefinition.DeclareElementMetaData("fVslider0", "tooltip", "percentage of processed signal in output signal");
		UIDefinition.AddElement(new FaustUIWriteableFloatElement(EFaustUIElementType.VerticalSlider, "wet_dry", new FaustVariableAccessor {
				ID = "fVslider0",
				SetValue = delegate(double val) { fVslider0 = val; },
				GetValue = delegate { return fVslider0; }
			}
			, 100.0, 0.0, 100.0, 1.0));
		UIDefinition.EndBox();
		
	}

	public void ClassInit(int sample_rate)
	{
		ChorusSIG0 sig0 = newChorusSIG0();
		sig0.instanceInitChorusSIG0(sample_rate);
		sig0.fillChorusSIG0(65536, ftbl0ChorusSIG0);
		deleteChorusSIG0(sig0);
		
	}
	
	public void InstanceConstants(int sample_rate)
	{
		fSampleRate = sample_rate;
		double fConst0 = Math.Min(192000.0, Math.Max(1.0, (double)(fSampleRate)));
		fConst1 = (0.01 * fConst0);
		fConst2 = (1.0 / fConst0);
		fConst3 = (1000.0 / fConst0);
		fConst4 = (0.0 - fConst3);
		
	}
	
	public void InstanceResetUserInterface()
	{
		fHslider0 = (double)(0.5);
		fVslider0 = (double)(100.0);
		fHslider1 = (double)(0.02);
		fHslider2 = (double)(2.0);
		
	}
	
	public void InstanceClear()
	{
		//for (int l0 = 0; (l0 < 2); l0 = (l0 + 1)) {
		//	fRec0_l0 = 0.0;
		//}
		//IOTA = 0;
		//for (int l1 = 0; (l1 < 131072); l1 = (l1 + 1)) {
		//	fVec0[l1] = 0.0;
		//}
		//for (int l2 = 0; (l2 < 2); l2 = (l2 + 1)) {
		//	fRec5_l2 = 0.0;
		//}
		//for (int l4 = 0; (l4 < 2); l4 = (l4 + 1)) {
		//	fRec1_l4 = 0.0;
		//}
		//for (int l5 = 0; (l5 < 2); l5 = (l5 + 1)) {
		//	fRec2_l5 = 0.0;
		//}
		//for (int l6 = 0; (l6 < 2); l6 = (l6 + 1)) {
		//	fRec3_l6 = 0.0;
		//}
		//for (int l7 = 0; (l7 < 2); l7 = (l7 + 1)) {
		//	fRec4_l7 = 0.0;
		//}
		
	}
	
	public void Init(int sample_rate)
	{
		ClassInit(sample_rate);
		InstanceInit(sample_rate);
	}
	
	public void InstanceInit(int sample_rate)
	{
		InstanceConstants(sample_rate);
		InstanceResetUserInterface();
		InstanceClear();
	}
	
	public void Compute(int count, double[][] inputs, double[][] outputs)
	{
		double[] input0 = inputs[0];
		double[] output0 = outputs[0];
		double fSlow0 = (0.0070000000000000062 * (double)(fHslider0));
		double fSlow1 = (0.01 * (double)(fVslider0));
		double fSlow2 = (double)(fHslider1);
		double fSlow3 = (fConst2 * (double)(fHslider2));
		double fSlow4 = (fSlow1 + (1.0 - fSlow1));
		for (int i = 0; (i < count); i = (i + 1)) {
			fRec0_0 = (fSlow0 + (0.99299999999999999 * fRec0_1));
			double fTemp0 = (double)(input0[i]);
			double fTemp1 = (fSlow1 * fTemp0);
			fVec0[(IOTA & 131071)] = fTemp1;
			fRec5_0 = (fSlow3 + (fRec5_1 - Math.Floor((fSlow3 + fRec5_1))));
			double fTemp2 = (65536.0 * (fRec5_0 - Math.Floor(fRec5_0)));
			double fTemp3 = Math.Floor(fTemp2);
			int iTemp4 = (int)(fTemp3);
			double fTemp5 = (fConst1 * ((fSlow2 * (((fTemp3 + (1.0 - fTemp2)) * ftbl0ChorusSIG0[(iTemp4 & 65535)]) + ((fTemp2 - fTemp3) * ftbl0ChorusSIG0[((iTemp4 + 1) & 65535)]))) + 1.0));
			double fTemp6 = (((fRec1_1 != 0.0)) ? (((((fRec2_1 > 0.0)?1:0) & ((fRec2_1 < 1.0)?1:0)) != 0) ? fRec1_1 : 0.0) : (((((fRec2_1 == 0.0)?1:0) & ((fTemp5 != fRec3_1)?1:0)) != 0) ? fConst3 : (((((fRec2_1 == 1.0)?1:0) & ((fTemp5 != fRec4_1)?1:0)) != 0) ? fConst4 : 0.0)));
			fRec1_0 = fTemp6;
			fRec2_0 = Math.Max(0.0, Math.Min(1.0, (fRec2_1 + fTemp6)));
			fRec3_0 = (((((fRec2_1 >= 1.0)?1:0) & ((fRec4_1 != fTemp5)?1:0)) != 0) ? fTemp5 : fRec3_1);
			fRec4_0 = (((((fRec2_1 <= 0.0)?1:0) & ((fRec3_1 != fTemp5)?1:0)) != 0) ? fTemp5 : fRec4_1);
			double fTemp7 = fVec0[((IOTA - (int)(Math.Min(65536.0, Math.Max(0.0, fRec3_0)))) & 131071)];
			output0[i] = (double)(((fRec0_0 * (fTemp7 + (fRec2_0 * (fVec0[((IOTA - (int)(Math.Min(65536.0, Math.Max(0.0, fRec4_0)))) & 131071)] - fTemp7)))) + (fSlow4 * fTemp0)));
			fRec0_1 = fRec0_0;
			IOTA = (IOTA + 1);
			fRec5_1 = fRec5_0;
			fRec1_1 = fRec1_0;
			fRec2_1 = fRec2_0;
			fRec3_1 = fRec3_0;
			fRec4_1 = fRec4_0;
		}
		
	}
	
};



