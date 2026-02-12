#ifndef SAMER_PROTOCOL_H
#define SAMER_PROTOCOL_H

#include <ESP32Servo.h>

// Movement speed levels in degrees per cycle
// Higher values = faster movement, adjust based on mechanical constraints
const float SPEED_LEVEL_1 = 1.4f; 
const float SPEED_LEVEL_2 = 2.8f;
const float SPEED_LEVEL_3 = 6.0f;

// Servo angle limits in degrees (0-180 range)
const float TILT_MIN = 95.0f;   
const float TILT_MAX = 175.0f;

const float PAN_MIN = 0.0f;
const float PAN_MAX = 180.0f;

// Default calibration positions (centered at 90° = 1500µs)
const float CALIBRATE_TILT = 90.0f; 
const float CALIBRATE_PAN = 90.0f; 

// Maps speed level (1-3) to actual velocity value
inline float getSpeedForLevel(int level) {
    switch(level) {
        case 1: return SPEED_LEVEL_1;
        case 2: return SPEED_LEVEL_2;
        case 3: return SPEED_LEVEL_3;
        default: return 0.0f;
    }
}

void processSamerPayload(const String &payload, Servo &tilt, Servo &pan);
void parseTiltCommand(const String &cmd, float &speed, bool &isStop, bool &isCalibrate);
void parsePanCommand(const String &cmd, float &speed, bool &isStop, bool &isCalibrate);

#endif