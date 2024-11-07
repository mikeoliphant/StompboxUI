# What is it?

Stompbox is a guitar amplification and effects application, arranged as a digital version of a guitar pedalboard.

This github repository is the front-end for the software (the core codebase is [stompbox](https://github.com/mikeoliphant/stompbox)), to be used either as a remote interface to an actual pedalboard implementation, or as a standalone VST plugin.

This is what it looks like running a Windows/Linux remote or as a VST plugin:

![stompbox](https://github.com/mikeoliphant/StompboxUI/assets/6710799/dd6e9349-ff0d-4437-af42-ef62f1096496)

This is what it looks like running as an Android remote:

![StompboxAndroid](https://github.com/mikeoliphant/StompboxUI/assets/6710799/3189e769-a28c-4e3b-8629-6846fb32de6c)

# Features

* Pedalboard-style layout (Amp/Cab with input chain and FX loop)
* [Neural Amp Modeler](https://github.com/sdatkinson/neural-amp-modeler) and [RTNeural](https://github.com/jatinchowdhury18/RTNeural) model support
* Cabinet impulse responses
* Tonestack/EQ
* Noise Gate
* Drive (clean boost, tube screamer)
* Time-based effects
  - Delay  
  - Reverb (algorithmic and convolution)
  - Compressor
  - Chorus
  - Phaser
  - Flanger
  - Tremolo
  - Vibrato
  - Wah/Auto-Wah
* Tuner
* Audio file player

# Platform Support

Stompbox currently can be run in the following ways:

* Headless on Linux (I use it on a [Raspberry Pi based pedalboard](https://www.youtube.com/watch?v=2I_bxxzQs2s))
* Remote UI on Windows, Linux or Android
* Standalone app on Windows
* VST3 plugin on Windows

# Installation

To run it as a VST3, you can simply download it from [the releases section of this repo](https://github.com/mikeoliphant/StompboxUI/releases/latest).

Simply extract the .zip file and copy the resulting folder to "C:\Program Files\Common Files\VST3".

# User Data Location

Files, such as NAM models, impulse responses, etc. go in your local user roaming AppData folder. Go to "%APPDATA%" in file explorer, and you should see a "stompbox" folder after the VST has been loaded at least once.

# Building From Source

Make sure you clone this github repo recursively:

```bash
git clone --recurse-submodules https://github.com/mikeoliphant/StompboxUI
```

Building should be straightforward using Visual Studio.

**NOTE:** Build and run the "StompboxImageProcessor" project first - it creates texture assets that are required for the main build.

