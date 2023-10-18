declare id "auto"; // ba.selector ve.crybaby / ve.autowah
declare name "auto";

//-----------------------------------------------
//     Auto-Wah
//-----------------------------------------------

import("stdfaust.lib"); //for ve.crybaby definition
import("guitarix.lib");

level = vslider("level", 0.5, 0, 1, 0.01);

process(x) = ve.autowah(level, x);
