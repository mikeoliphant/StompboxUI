import("stdfaust.lib"); 

chumpPreamp = pre : fi.iir((b0/a0,b1/a0,b2/a0,b3/a0,b4/a0),(a1/a0,a2/a0,a3/a0,a4/a0)) : redeyechumppreclip with {
    LogPot(a, x) = ba.if(a, (exp(a * x) - 1) / (exp(a) - 1), x);
    Inverted(b, x) = ba.if(b, 1 - x, x);
    s = 0.993;
    fs = float(ma.SR);
    pre = _;


    Gain = vslider("Gain[name:Gain]", 0.5, 0, 1, 0.01) : Inverted(0) : LogPot(3) : si.smooth(s);


    Tone = vslider("Tone[name:Tone]", 0.5, 0, 1, 0.01) : Inverted(0) : si.smooth(s);

    b0 = Gain*Tone*pow(fs,2)*(-1.42098357291903e-14*fs - 1.89464476389205e-13) + Gain*fs*(fs*(fs*(-2.84196714583805e-18*fs - 2.87986004111589e-15) - 7.14281075987297e-12) - 9.47322381946023e-11);

    b1 = 2.84196714583805e-14*Gain*Tone*pow(fs,3) + Gain*fs*(pow(fs,2)*(1.13678685833522e-17*fs + 5.75972008223179e-15) - 1.89464476389205e-10);

    b2 = 3.78928952778409e-13*Gain*Tone*pow(fs,2) + Gain*pow(fs,2)*(-1.70518028750283e-17*pow(fs,2) + 1.42856215197459e-11);

    b3 = -2.84196714583805e-14*Gain*Tone*pow(fs,3) + Gain*fs*(pow(fs,2)*(1.13678685833522e-17*fs - 5.75972008223179e-15) + 1.89464476389205e-10);

    b4 = Gain*Tone*pow(fs,2)*(1.42098357291903e-14*fs - 1.89464476389205e-13) + Gain*fs*(fs*(fs*(-2.84196714583805e-18*fs + 2.87986004111589e-15) - 7.14281075987297e-12) + 9.47322381946023e-11);

    a0 = Gain*(Gain*pow(fs,2)*(fs*(-4.83357544584368e-19*fs - 4.95743008549253e-16) - 1.23854639648849e-14) + fs*(fs*(fs*(4.99938975959391e-19*fs + 5.25223533960928e-16) + 2.55941946006592e-14) + 3.09636599122122e-13)) + fs*(fs*(fs*(4.99938975959391e-20*fs + 3.02491841375788e-16) + 1.40186442450682e-13) + 6.50692145985754e-12) + 7.74091497805305e-11;

    a1 = Gain*(Gain*pow(fs,3)*(1.93343017833747e-18*fs + 9.91486017098506e-16) + fs*(pow(fs,2)*(-1.99975590383756e-18*fs - 1.05044706792186e-15) + 6.19273198244244e-13)) + fs*(pow(fs,2)*(-1.99975590383756e-19*fs - 6.04983682751577e-16) + 1.30138429197151e-11) + 3.09636599122122e-10;

    a2 = Gain*(Gain*pow(fs,2)*(-2.90014526750621e-18*pow(fs,2) + 2.47709279297698e-14) + pow(fs,2)*(2.99963385575635e-18*pow(fs,2) - 5.11883892013183e-14)) + pow(fs,2)*(2.99963385575635e-19*pow(fs,2) - 2.80372884901364e-13) + 4.64454898683183e-10;

    a3 = Gain*(Gain*pow(fs,3)*(1.93343017833747e-18*fs - 9.91486017098506e-16) + fs*(pow(fs,2)*(-1.99975590383756e-18*fs + 1.05044706792186e-15) - 6.19273198244244e-13)) + fs*(pow(fs,2)*(-1.99975590383756e-19*fs + 6.04983682751577e-16) - 1.30138429197151e-11) + 3.09636599122122e-10;

    a4 = Gain*(Gain*pow(fs,2)*(fs*(-4.83357544584368e-19*fs + 4.95743008549253e-16) - 1.23854639648849e-14) + fs*(fs*(fs*(4.99938975959391e-19*fs - 5.25223533960928e-16) + 2.55941946006592e-14) - 3.09636599122122e-13)) + fs*(fs*(fs*(4.99938975959391e-20*fs - 3.02491841375788e-16) + 1.40186442450682e-13) - 6.50692145985754e-12) + 7.74091497805305e-11;
};

redeyechumppreclip = _<: ba.if(signbit(_), redeyechumppre_neg_clip, redeyechumppre_clip) :>_ with {

    signbit = ffunction(int signbit(float), "math.h", "");

    redeyechumppre_clip = ffunction(float redeyechumppreclip(float), "redeyechumppre_table.h", "");

    redeyechumppre_neg_clip = ffunction(float redeyechumppre_negclip(float), "redeyechumppre_neg_table.h", "");

};

process = chumpPreamp;