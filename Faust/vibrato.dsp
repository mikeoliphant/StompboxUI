declare id   "phaser_mono";
declare name "Phaser Mono";
declare category "Modulation";

import("stdfaust.lib");

vibrato_mono(sections, fb, width, frqmin, fratio, frqmax, speed) = 
 (+ : seq(i, sections, ap2p(R,th(i)))) ~ *(fb)
with {
     // second-order resonant digital allpass given fi.pole radius and angle:
     ap2p(R,th) = fi.tf2(a2, a1, 1, a1, a2) with {
       a2 = R^2;
       a1 = -2*R*cos(th);
     };
     R = exp(-pi*width/ma.SR);
     osc = os.oscrs(speed);
     lfo = (1-osc)/2; // in [0,1]
     pi = 4*atan(1);
     thmin = 2*pi*frqmin/ma.SR;
     thmax = 2*pi*frqmax/ma.SR;
     th1 = thmin + (thmax-thmin)*lfo;
     th(i) = (fratio^(i+1))*th1;
};

process = _ <: vibrato_mono(stages, fb, width, frqmin, fratio, frqmax, freq)
with {
  stages = 2;
  freq   = fvariable(float freq, <math.h>);
  fb	 = fvariable(float fb, <math.h>);
  width  = fvariable(float width, <math.h>);
  frqmin = fvariable(float frqmin, <math.h>);
  frqmax = fvariable(float frqmax, <math.h>);
  fratio = fvariable(float fratio, <math.h>);
};