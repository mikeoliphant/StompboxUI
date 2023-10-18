declare id "dide";
declare name "Digital Delay";
declare shortname "Digi Delay";
declare category "Echo / Delay";
declare description "Digital Delay";

import("stdfaust.lib");
import("guitarix.lib");

dide         = _<:*(dry),(delx : *(wet)):>_ with {
  delx    = _<:(digd), !:>_;

  digd       = (+:(delayed:lback))~(fback) with {
    fback = _<:feed:>_;
    lback = _<:*(level) : fi.highpass(2,hifr1) : fi.lowpass(2,lofr1):>_;
    feed     = *(feedback);
    delayed  = de.sdelay(N, interp, dtime) with {
      dtime  = fvariable(float delay, <math.h>)*ma.SR/1000.0;
      interp = 100*ma.SR/1000.0;
      N      = int(192000) ;
    };

    level    = fvariable(float level, <math.h>);
    feedback = fvariable(float feedback, <math.h>);
    hifr1    = fvariable(float hipass, <math.h>);
    lofr1    = fvariable(float lowpass, <math.h>);
    };

  wet      = fvariable(float wet, <math.h>);
  dry        = 1 ;
};

process      = dide;
