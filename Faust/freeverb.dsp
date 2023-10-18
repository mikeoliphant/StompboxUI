declare name "Freeverb";
declare category "Reverb";

declare version 	"0.01";
declare author 		"brummer";
declare license 	"BSD";
declare copyright 	"(c)brummer 2008";

import("stdfaust.lib");

import("guitarix.lib");

/*-----------------------------------------------
		freeverb  by "Grame"
  -----------------------------------------------*/

// Filter Parameters

scaleSampleRate(sample) = sample * (ma.SR / 44100);

combtuningL1	= scaleSampleRate(1116);
combtuningL2	= scaleSampleRate(1188);
combtuningL3	= scaleSampleRate(1277);
combtuningL4	= scaleSampleRate(1356);
combtuningL5	= scaleSampleRate(1422);
combtuningL6	= scaleSampleRate(1491);
combtuningL7	= scaleSampleRate(1557);
combtuningL8	= scaleSampleRate(1617);

allpasstuningL1	= scaleSampleRate(556);
allpasstuningL2	= scaleSampleRate(441);
allpasstuningL3	= scaleSampleRate(341);
allpasstuningL4	= scaleSampleRate(225);

roomsizeSlider 	= vslider("RoomSize", 0.5, 0, 1, 0.025)*0.28 + 0.7;
decayslider 	= vslider("Decay",5, 0, 10, 0.1);
combfeed 	= roomsizeSlider;
//wetslider 	= 0.5 + vslider("wet_dry[name:wet/dry]", 0, -0.5, 0.5, 0.1);
wet_dry = vslider("wet_dry[name:wet/dry]",  50, 0, 100, 1) : /(100);
dry = 1 - wet_dry;

// Reverb components

monoReverb(fb1, fb2, damp, spread)
	= _ <:	comb(combtuningL1+spread, fb1, damp),
			comb(combtuningL2+spread, fb1, damp),
			comb(combtuningL3+spread, fb1, damp),
			comb(combtuningL4+spread, fb1, damp),
			comb(combtuningL5+spread, fb1, damp),
			comb(combtuningL6+spread, fb1, damp),
			comb(combtuningL7+spread, fb1, damp),
			comb(combtuningL8+spread, fb1, damp)
		+>
		 	allpass (allpasstuningL1+spread, fb2)
		:	allpass (allpasstuningL2+spread, fb2)
		:	allpass (allpasstuningL3+spread, fb2)
		:	allpass (allpasstuningL4+spread, fb2)
		;

//----------------------------------------------------------------

fxctrl(g,w,Fx) =  _ <: (*(g) <: _ + Fx ), *(1-w) +> _;
process = _<:*(dry),(*(wet_dry):fxctrl(0.015,wet_dry, monoReverb(combfeed, 0.5, 1 - (decayslider / 10), 23))):>_;
