import("stdfaust.lib");
import("guitarix.lib");

tubestageNew(tb,vplus,divider,Rp,fck,Rk,Vk0) = anti_aliase : tube : dcblock with {
    lpfk = fi.lowpass(1,fck);
    anti_aliase = fi.lowpass(3,ma.SR/2.1);
   // VkC = Vk0 * (Rp + Ranode(tb)) / Rk;
   // Vp = -(Vk0) : Ftube(tb);
   // tubeVp = Vp <: +(VkC - vplus);
    VkC = Vk0 * (Rp/Rk);
    tubeVp = -(Vk0) : Ftube(tb) : +(VkC-vplus);
    tube(x) = x : (+ : (tubeVp)) ~ (x*Rk/(Rp + Ranode(tb)) : lpfk) : /(divider);
    dcblock = fi.dcblockerat(1.0);
};

// SHP : This version allows user setting of Vplus and Divider
// Useful as many circuits do not have 100K default here ( marshall preamps, all power tubes etc )
tubestageOld(tb,vplus,divider,Rp,fck,Rk,Vk0) = anti_aliase : tube : hpf with {
    lpfk = fi.lowpass(1,fck);
    anti_aliase = fi.lowpass(3,ma.SR/2.1);
    VkC = Vk0 * (Rp/Rk);
    tube = (+ : -(Vk0) : Ftube(tb) : +(VkC-vplus)) ~ (*(Rk/Rp) : lpfk) : /(divider);
    hpf = fi.highpass(1,31.0);  // Consider removing this
};

gain = vslider("gain",-6,-20,20,0.1) : ba.db2linear : smoothi(0.999);

vplus =  fvariable(float vplus, <math.h>);
divider = fvariable(float divider, <math.h>);
Rp = fvariable(float Rp, <math.h>);
fck = fvariable(float fck, <math.h>);
Rk = fvariable(float Rk, <math.h>);
Vk0 = fvariable(float Vk0, <math.h>);

process = *(gain) : tubestageOld(TB_12AX7_68k, vplus, divider, Rp, fck, Rk, Vk0) : fi.lowpass(1,6531.0);
