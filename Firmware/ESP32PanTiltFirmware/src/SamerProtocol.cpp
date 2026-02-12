#include "SamerProtocol.h"

// Current servo positions in degrees
float currentTilt = CALIBRATE_TILT;
float currentPan = CALIBRATE_PAN;

// Active movement velocities (degrees per cycle)
float tiltSpeed = 0.0f;
float panSpeed = 0.0f;

// Converts angle (0-180°) to servo pulse width (500-2500µs)
int toUs(float angle) {
    return (int)(500 + (angle * (2500 - 500) / 180.0));
}

// Parses 4-character command string into movement parameters
// Format: [Direction][XXX][Speed_Level] (e.g. "FWD3", "STOP")
void parseCommand(const String &cmd, float &speed, bool &isStop, bool &isCalibrate) { 
      speed = 0.0f; isStop = false; isCalibrate = false;

       // Validate minimum command length
       if (cmd.length() < 4) { isStop = true; return; }  

       char dir = cmd[0];
       int lvl = isDigit(cmd[3]) ? cmd[3] - '0' : 0;
       float velocity = getSpeedForLevel(lvl);

       switch(dir) {
        case 'S':   isStop = true; break;       //Stop command
        case 'T':   isCalibrate = true; break;  //Return to center

        case 'L':  case 'B':  speed = velocity; break;  // Left/Back (positive)
        case 'R':  case 'F':  speed = -velocity; break; // Right/Forward (negative)
       
       }
    }


// Main protocol handler - processes 8-char payload and updates servos
// Format: [TILT_CMD(4)][PAN_CMD(4)] (e.g. "FWD3RGT2")
void processSamerPayload(const String &payload, Servo &tilt, Servo &pan) {

            if(payload.length() < 8) return;

            // Tilt servo power management to reduce idle buzzing
            static unsigned long lastTiltMoveTime = 0;
            static bool isTiltAttached = true;
            static float lastWrittenTilt = -1.0f; 

            // Parse tilt command (first 4 characters)
            bool tStop, tCal;
            parseCommand(payload.substring(0, 4), tiltSpeed, tStop, tCal); 

            if(tCal) {
                currentTilt = CALIBRATE_TILT;
                tiltSpeed = 0;
            }

            else if(tStop) tiltSpeed = 0;

            // Parse pan command (last 4 characters)
            bool pStop, pCal;
            parseCommand(payload.substring(4, 8), panSpeed, pStop, pCal);

            if(pCal) {
                currentPan = CALIBRATE_PAN;
                panSpeed = 0;
            }

            else if(pStop) panSpeed = 0;

            // Update positions with current velocities
            currentTilt += tiltSpeed;
            currentPan += panSpeed;

            currentTilt = constrain(currentTilt, TILT_MIN, TILT_MAX);
            currentPan = constrain(currentPan, PAN_MIN, PAN_MAX);

            // Tilt servo attach/detach logic for power saving
            if(tiltSpeed != 0 || tCal || abs(currentTilt - lastWrittenTilt) > 0.1) {
                if(!isTiltAttached) {
                    tilt.attach(18, 500, 2500);
                    isTiltAttached = true;
                }

                lastTiltMoveTime = millis();
                lastWrittenTilt = currentTilt;
            }
            
            // Detach after 500ms of inactivity to prevent buzzing
            else if(isTiltAttached && (millis() - lastTiltMoveTime > 500)){
                tilt.detach();
                isTiltAttached = false;
            }

            // Write final positions to servos
            if(isTiltAttached) tilt.writeMicroseconds(toUs(currentTilt));
            pan.writeMicroseconds(toUs(currentPan));
       }

