declare id "auto"; // ba.selector ve.crybaby / ve.autowah
declare name "auto";

//-----------------------------------------------
//     Auto-Wah
//-----------------------------------------------

import("stdfaust.lib"); //for ve.crybaby definition
import("guitarix.lib");

wah = vslider("wah", 0.5, 0, 1, 0.01);

process(x) = ve.crybaby(wah, x);
