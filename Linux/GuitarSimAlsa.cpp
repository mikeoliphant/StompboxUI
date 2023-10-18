#include "ALSADevices.hpp"
#include <iostream>
#include <cstring>

#define SAMPLING_RATE 44100
#define CHANNELS 2
#define FRAMES_PER_PERIOD 32
#define FORMAT SND_PCM_FORMAT_S32_LE

int main() {
    std::cout << "Create for read\n";
    ALSACaptureDevice microphone("hw:GT1,0", SAMPLING_RATE, CHANNELS, FRAMES_PER_PERIOD, FORMAT);

    std::cout << "Create for write\n";
    ALSAPlaybackDevice speaker("hw:GT1,0", SAMPLING_RATE, CHANNELS, FRAMES_PER_PERIOD, FORMAT);

    std::cout << "Open for read\n";
    microphone.open();

    std::cout << "Open for write\n";
    speaker.open();

    char* buffer = microphone.allocate_buffer();
    unsigned int frames_captured, frames_played;

    do {
        frames_captured = microphone.capture_into_buffer(buffer, FRAMES_PER_PERIOD);
        frames_played = speaker.play_from_buffer(buffer, FRAMES_PER_PERIOD);
        std::cout << "Captured,Played ---> " << frames_captured << "," << frames_played << std::endl;
    } while (1);

    microphone.close();
    speaker.close();

    return 0;
}