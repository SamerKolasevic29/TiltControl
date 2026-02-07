#include "SamerProtocol.h"

//Global state vars
int currentTilt = CALIBRATE_TILT;
int currentPan = CALIBRATE_PAN;

//Current speed (0 = stop)
int tiltSpeed = 0;
int panSpeed = 0;

// HELPER FUNCTION - Parse direction and speed
void parseCommand(const String &cmd, int &speed, bool &isForward, bool &isStop, bool &isCalibrate) {
    // Default values
    speed = 0;
    isForward = false;
    isStop = false;
    isCalibrate = false;
    
    // Validation
    if (cmd.length() < 4) {
        isStop = true;
        return;
    }
    
    // Parse direction (first char)
    char firstChar = cmd[0];
    
    // Parse speed level (4th char)
    char speedChar = cmd[3];
    int level = 0;
    if (isDigit(speedChar)) {
        level = speedChar - '0';
    }
    
    // Main switch - Direction handling
    switch(firstChar) {
        case 'S':  // STOP
            isStop = true;
            break;
            
        case 'T':  // TEST (calibrate)
            isCalibrate = true;
            break;
            
        case 'F':  // FWD (forward)
            isForward = true;
            speed = getSpeedForLevel(level);
            break;
            
        case 'B':  // BCK (backward)
            isForward = false;
            speed = -getSpeedForLevel(level);  // Negative for backward
            break;
            
        case 'R':  // RGT (right)
            isForward = true;
            speed = getSpeedForLevel(level);
            break;
            
        case 'L':  // LFT (left)
            isForward = false;
            speed = -getSpeedForLevel(level);  // Negative for left
            break;
            
        default:
            // Unknown command → treat as STOP
            isStop = true;
            break;
    }
}



void processSamerPayload(const String &payload, Servo &tilt, Servo &pan) {
    if (payload.length() < 8) return;
    
    // --- PARSE TILT COMMAND (first 4 chars) ---
    String tiltCmd = payload.substring(0, 4);
    bool tiltForward, tiltStop, tiltCalibrate;
    parseCommand(tiltCmd, tiltSpeed, tiltForward, tiltStop, tiltCalibrate);
    
    if (tiltCalibrate) {
        currentTilt = CALIBRATE_TILT;
        tiltSpeed = 0;
    }
    else if (tiltStop) {
        tiltSpeed = 0;
    }
    
    // --- PARSE PAN COMMAND (last 4 chars) ---
    String panCmd = payload.substring(4, 8);
    bool panForward, panStop, panCalibrate;
    parseCommand(panCmd, panSpeed, panForward, panStop, panCalibrate);
    
    if (panCalibrate) {
        currentPan = CALIBRATE_PAN;
        panSpeed = 0;
    }
    else if (panStop) {
        panSpeed = 0;
    }
    
    // --- UPDATE POSITIONS (calling in every loop) ---
    currentTilt += tiltSpeed;
    currentPan += panSpeed;
    
    // --- CONSTRAIN SA SAFE LIMITS ---
    currentTilt = constrain(currentTilt, TILT_MIN, TILT_MAX);
    currentPan = constrain(currentPan, PAN_MIN, PAN_MAX);
    
    // --- WRITE TO SERVOS ---
    tilt.write(currentTilt);
    pan.write(currentPan);
}