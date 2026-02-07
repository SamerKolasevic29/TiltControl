#ifndef SAMER_PROTOCOL_H
#define SAMER_PROTOCOL_H

#include <ESP32Servo.h>

//speed levels (degrees per update)
const int SPEED_LEVEL_1 = 1;    //-precise 
const int SPEED_LEVEL_2 = 3;    //-balanced
const int SPEED_LEVEL_3 = 6;    //-agressive

//safe angle constrains (fine-tune)
const int TILT_MIN = 40;
const int TILT_MAX = 120;
const int PAN_MIN = 15;
const int PAN_MAX = 165;

//calibration positions for each servo
const int CALIBRATE_TILT = 40;
const int CALIBRATE_PAN = 90;

// Helper function - extracted for DRY (Don't Repeat Yourself)
inline int getSpeedForLevel(int level) {
    switch(level) {
        case 1: return SPEED_LEVEL_1;
        case 2: return SPEED_LEVEL_2;
        case 3: return SPEED_LEVEL_3;
        default: return 0;
    }
}

//MAIN FUNCTION - command parsing and execution
void processSamerPayload(const String &payload, Servo &tilt, Servo &pan);
//Helper function - parsing in main function
void parseCommand(const String &cmd, int &speed, bool &isForward, bool &isStop, bool &isCalibrate);


#endif