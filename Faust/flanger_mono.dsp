declare id 		"flanger_mono";
declare name            "Flanger Mono";
declare category        "Modulation";
declare license 	"BSD";

import("stdfaust.lib");
import("guitarix.lib");

flanger_mono(dmax,curdel,depth,fb,invert)
  = _ <: _, (- : de.fdelay(dmax,curdel)) ~ *(fb) : _,
  *(0-depth)
  : + : *(0.5);

flangermonogx = flanger_mono(2048, curdel, depth, fb)
with {
	  lfol = os.oscrs; 
	  dflange = 0.001 * ma.SR *  10.0;
	  odflange = 0.001 * ma.SR *  1.0;
	  freq	 = hslider("freq [unit:Hz]", 0.2, 0, 5, 0.01);
	  depth = hslider("depth", 1, 0, 1, 0.01);
	  fb = hslider("feedback", 0.5, 0, 1, 0.01);
	  curdel = odflange+dflange*(1 + lfol(freq))/2; 
  };
  
process =  _<: flangermonogx :>_;
