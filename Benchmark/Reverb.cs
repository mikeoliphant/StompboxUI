/* ------------------------------------------------------------
author: "brummer"
copyright: "(c)brummer 2008"
license: "BSD"
name: "Freeverb"
version: "0.01"
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


public class Reverb : dsp, IFaustDSP
{
	
	
	double fVslider0;
	double fVslider1;
	double fVslider2;
	double[] fRec9 = new double[2];
	int IOTA;
	double[] fVec0 = new double[8192];
	int fSampleRate;
	int iConst1;
	double[] fRec8 = new double[2];
	double[] fRec11 = new double[2];
	double[] fVec1 = new double[8192];
	int iConst2;
	double[] fRec10 = new double[2];
	double[] fRec13 = new double[2];
	double[] fVec2 = new double[8192];
	int iConst3;
	double[] fRec12 = new double[2];
	double[] fRec15 = new double[2];
	double[] fVec3 = new double[8192];
	int iConst4;
	double[] fRec14 = new double[2];
	double[] fRec17 = new double[2];
	double[] fVec4 = new double[8192];
	int iConst5;
	double[] fRec16 = new double[2];
	double[] fRec19 = new double[2];
	double[] fVec5 = new double[8192];
	int iConst6;
	double[] fRec18 = new double[2];
	double[] fRec21 = new double[2];
	double[] fVec6 = new double[8192];
	int iConst7;
	double[] fRec20 = new double[2];
	double[] fRec23 = new double[2];
	double[] fVec7 = new double[8192];
	int iConst8;
	double[] fRec22 = new double[2];
	double[] fVec8 = new double[4096];
	int iConst9;
	double[] fRec6 = new double[2];
	double[] fVec9 = new double[2048];
	int iConst10;
	double[] fRec4 = new double[2];
	double[] fVec10 = new double[2048];
	int iConst11;
	double[] fRec2 = new double[2];
	double[] fVec11 = new double[1024];
	int iConst12;
	double[] fRec0 = new double[2];
	
	public Reverb()
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
		MetaData.Declare("author", "brummer");
		MetaData.Declare("category", "Reverb");
		MetaData.Declare("compile_options", "-lang csharp -es 1 -double -ftz 0");
		MetaData.Declare("copyright", "(c)brummer 2008");
		MetaData.Declare("filename", "FreeVerb.dsp");
		MetaData.Declare("license", "BSD");
		MetaData.Declare("maths.lib/author", "GRAME");
		MetaData.Declare("maths.lib/copyright", "GRAME");
		MetaData.Declare("maths.lib/license", "LGPL with exception");
		MetaData.Declare("maths.lib/name", "Faust Math Library");
		MetaData.Declare("maths.lib/version", "2.3");
		MetaData.Declare("name", "Freeverb");
		MetaData.Declare("platform.lib/name", "Generic Platform Library");
		MetaData.Declare("platform.lib/version", "0.1");
		MetaData.Declare("version", "0.01");
	}

	
	void BuildUserInterface()
	{
		UIDefinition.StartBox(new FaustBoxElement(EFaustUIElementType.VerticalBox, "Freeverb"));
		UIDefinition.AddElement(new FaustUIWriteableFloatElement(EFaustUIElementType.VerticalSlider, "Decay", new FaustVariableAccessor {
				ID = "fVslider2",
				SetValue = delegate(double val) { fVslider2 = val; },
				GetValue = delegate { return fVslider2; }
			}
			, 5.0, 0.0, 10.0, 0.10000000000000001));
		UIDefinition.AddElement(new FaustUIWriteableFloatElement(EFaustUIElementType.VerticalSlider, "RoomSize", new FaustVariableAccessor {
				ID = "fVslider1",
				SetValue = delegate(double val) { fVslider1 = val; },
				GetValue = delegate { return fVslider1; }
			}
			, 0.5, 0.0, 1.0, 0.025000000000000001));
		UIDefinition.DeclareElementMetaData("fVslider0", "name", "wet/dry");
		UIDefinition.AddElement(new FaustUIWriteableFloatElement(EFaustUIElementType.VerticalSlider, "wet_dry", new FaustVariableAccessor {
				ID = "fVslider0",
				SetValue = delegate(double val) { fVslider0 = val; },
				GetValue = delegate { return fVslider0; }
			}
			, 50.0, 0.0, 100.0, 1.0));
		UIDefinition.EndBox();
		
	}

	public void ClassInit(int sample_rate)
	{
		
	}
	
	public void InstanceConstants(int sample_rate)
	{
		fSampleRate = sample_rate;
		double fConst0 = Math.Min(192000.0, Math.Max(1.0, (double)(fSampleRate)));
		iConst1 = (int)(((0.036666666666666667 * fConst0) + 23.0));
		iConst2 = (int)(((0.035306122448979592 * fConst0) + 23.0));
		iConst3 = (int)(((0.03380952380952381 * fConst0) + 23.0));
		iConst4 = (int)(((0.032244897959183672 * fConst0) + 23.0));
		iConst5 = (int)(((0.030748299319727893 * fConst0) + 23.0));
		iConst6 = (int)(((0.026938775510204082 * fConst0) + 23.0));
		iConst7 = (int)(((0.025306122448979593 * fConst0) + 23.0));
		iConst8 = (int)(((0.028956916099773244 * fConst0) + 23.0));
		iConst9 = (int)(((0.012607709750566893 * fConst0) + 23.0));
		iConst10 = (int)(((0.01 * fConst0) + 23.0));
		iConst11 = (int)(((0.0077324263038548759 * fConst0) + 23.0));
		iConst12 = (int)(((0.0051020408163265311 * fConst0) + 23.0));
		
	}
	
	public void InstanceResetUserInterface()
	{
		fVslider0 = (double)(50.0);
		fVslider1 = (double)(0.5);
		fVslider2 = (double)(5.0);
		
	}
	
	public void InstanceClear()
	{
		for (int l0 = 0; (l0 < 2); l0 = (l0 + 1)) {
			fRec9[l0] = 0.0;
		}
		IOTA = 0;
		for (int l1 = 0; (l1 < 8192); l1 = (l1 + 1)) {
			fVec0[l1] = 0.0;
		}
		for (int l2 = 0; (l2 < 2); l2 = (l2 + 1)) {
			fRec8[l2] = 0.0;
		}
		for (int l3 = 0; (l3 < 2); l3 = (l3 + 1)) {
			fRec11[l3] = 0.0;
		}
		for (int l4 = 0; (l4 < 8192); l4 = (l4 + 1)) {
			fVec1[l4] = 0.0;
		}
		for (int l5 = 0; (l5 < 2); l5 = (l5 + 1)) {
			fRec10[l5] = 0.0;
		}
		for (int l6 = 0; (l6 < 2); l6 = (l6 + 1)) {
			fRec13[l6] = 0.0;
		}
		for (int l7 = 0; (l7 < 8192); l7 = (l7 + 1)) {
			fVec2[l7] = 0.0;
		}
		for (int l8 = 0; (l8 < 2); l8 = (l8 + 1)) {
			fRec12[l8] = 0.0;
		}
		for (int l9 = 0; (l9 < 2); l9 = (l9 + 1)) {
			fRec15[l9] = 0.0;
		}
		for (int l10 = 0; (l10 < 8192); l10 = (l10 + 1)) {
			fVec3[l10] = 0.0;
		}
		for (int l11 = 0; (l11 < 2); l11 = (l11 + 1)) {
			fRec14[l11] = 0.0;
		}
		for (int l12 = 0; (l12 < 2); l12 = (l12 + 1)) {
			fRec17[l12] = 0.0;
		}
		for (int l13 = 0; (l13 < 8192); l13 = (l13 + 1)) {
			fVec4[l13] = 0.0;
		}
		for (int l14 = 0; (l14 < 2); l14 = (l14 + 1)) {
			fRec16[l14] = 0.0;
		}
		for (int l15 = 0; (l15 < 2); l15 = (l15 + 1)) {
			fRec19[l15] = 0.0;
		}
		for (int l16 = 0; (l16 < 8192); l16 = (l16 + 1)) {
			fVec5[l16] = 0.0;
		}
		for (int l17 = 0; (l17 < 2); l17 = (l17 + 1)) {
			fRec18[l17] = 0.0;
		}
		for (int l18 = 0; (l18 < 2); l18 = (l18 + 1)) {
			fRec21[l18] = 0.0;
		}
		for (int l19 = 0; (l19 < 8192); l19 = (l19 + 1)) {
			fVec6[l19] = 0.0;
		}
		for (int l20 = 0; (l20 < 2); l20 = (l20 + 1)) {
			fRec20[l20] = 0.0;
		}
		for (int l21 = 0; (l21 < 2); l21 = (l21 + 1)) {
			fRec23[l21] = 0.0;
		}
		for (int l22 = 0; (l22 < 8192); l22 = (l22 + 1)) {
			fVec7[l22] = 0.0;
		}
		for (int l23 = 0; (l23 < 2); l23 = (l23 + 1)) {
			fRec22[l23] = 0.0;
		}
		for (int l24 = 0; (l24 < 4096); l24 = (l24 + 1)) {
			fVec8[l24] = 0.0;
		}
		for (int l25 = 0; (l25 < 2); l25 = (l25 + 1)) {
			fRec6[l25] = 0.0;
		}
		for (int l26 = 0; (l26 < 2048); l26 = (l26 + 1)) {
			fVec9[l26] = 0.0;
		}
		for (int l27 = 0; (l27 < 2); l27 = (l27 + 1)) {
			fRec4[l27] = 0.0;
		}
		for (int l28 = 0; (l28 < 2048); l28 = (l28 + 1)) {
			fVec10[l28] = 0.0;
		}
		for (int l29 = 0; (l29 < 2); l29 = (l29 + 1)) {
			fRec2[l29] = 0.0;
		}
		for (int l30 = 0; (l30 < 1024); l30 = (l30 + 1)) {
			fVec11[l30] = 0.0;
		}
		for (int l31 = 0; (l31 < 2); l31 = (l31 + 1)) {
			fRec0[l31] = 0.0;
		}
		
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
		double fSlow0 = (double)(fVslider0);
		double fSlow1 = (0.00014999999999999999 * fSlow0);
		double fSlow2 = ((0.28000000000000003 * (double)(fVslider1)) + 0.69999999999999996);
		double fSlow3 = (0.10000000000000001 * (double)(fVslider2));
		double fSlow4 = (1.0 - fSlow3);
		double fSlow5 = (1.0 - (0.01 * fSlow0));
		double fSlow6 = (fSlow5 + (fSlow0 * ((0.01 * fSlow5) + 0.00014999999999999999)));
		for (int i = 0; (i < count); i = (i + 1)) {
			double fTemp0 = (double)(input0[i]);
			double fTemp1 = (fSlow1 * fTemp0);
			fVec0[(IOTA & 8191)] = (fTemp1 + (fSlow2 * fRec9[0]));
			fVec1[(IOTA & 8191)] = (fTemp1 + (fSlow2 * fRec11[0]));
			fVec2[(IOTA & 8191)] = (fTemp1 + (fSlow2 * fRec13[0]));
			fVec3[(IOTA & 8191)] = (fTemp1 + (fSlow2 * fRec15[0]));
			fVec4[(IOTA & 8191)] = (fTemp1 + (fSlow2 * fRec17[0]));
			fVec5[(IOTA & 8191)] = (fTemp1 + (fSlow2 * fRec19[0]));
			fVec6[(IOTA & 8191)] = (fTemp1 + (fSlow2 * fRec21[0]));
			fVec7[(IOTA & 8191)] = (fTemp1 + (fSlow2 * fRec23[0]));
			double fTemp2 = (fRec8[0] + (fRec10[0] + (fRec12[0] + (fRec14[0] + (fRec16[0] + (fRec18[0] + (fRec20[0] + fRec22[0])))))));
			fVec8[(IOTA & 4095)] = ((0.5 * fRec6[1]) + fTemp2);
			double fRec7 = (fRec6[1] - fTemp2);
			fVec9[(IOTA & 2047)] = (fRec7 + (0.5 * fRec4[1]));
			double fRec5 = (fRec4[1] - fRec7);
			fVec10[(IOTA & 2047)] = (fRec5 + (0.5 * fRec2[1]));
			double fRec3 = (fRec2[1] - fRec5);
			fVec11[(IOTA & 1023)] = (fRec3 + (0.5 * fRec0[1]));
			double fRec1 = (fRec0[1] - fRec3);
			output0[i] = (double)((fRec1 + (fSlow6 * fTemp0)));
			IOTA = (IOTA + 1);
		}
		
	}
	
};



