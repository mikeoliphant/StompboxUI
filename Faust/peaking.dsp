import("stdfaust.lib");

peq = fi.peak_eq(l,f,b)
with {
        l = vslider("[2]Level[unit:db]",0,-15,15,0.01) : si.smoo;
        f = fconstant(float freq, <math.h>);
        b = f / fconstant(float q, <math.h>);
};

process = peq;