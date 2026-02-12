#include <Arduino.h>
#include <WiFi.h>
#include <WiFiUdp.h>
#include <ESP32Servo.h>
#include "SamerProtocol.h"

// Network configuration
const char* ssid = "žnj";
const char* password = "ovvoonno";
const unsigned int localUdpPort = 4210;

// Hardware pin definitions
const int tiltPin = 18;
const int panPin = 19;

WiFiUDP udp;
Servo tiltServo;
Servo panServo;
char incomingPacket[255];
String lastCommand = "STOPSTOP";     // Default idle state

void setup() {
    Serial.begin(115200);
    
    // Initialize servos with 500-2500µs range (0-180° mapping)
    tiltServo.setPeriodHertz(50);
    tiltServo.attach(tiltPin, 500, 2500);
    
    panServo.setPeriodHertz(50);
    panServo.attach(panPin, 500, 2500);
    
    // Center both servos at startup (90° = 1500µs)
    tiltServo.writeMicroseconds(1500);
    panServo.writeMicroseconds(1500);

    // Establish WiFi connection
    Serial.print("Connecting to WiFi");
    WiFi.begin(ssid, password);
    while (WiFi.status() != WL_CONNECTED) {
        delay(250); Serial.print(".");
    }
    Serial.println("\nConnected! IP: ");
    Serial.println(WiFi.localIP());

    // Start UDP listener
    udp.begin(localUdpPort);
}

void loop() {
    // Poll for incoming UDP packets
    int packetSize = udp.parsePacket();
    if (packetSize) {
        int len = udp.read(incomingPacket, 255);
        if (len > 0) {
            incomingPacket[len] = 0;
            lastCommand = String(incomingPacket);
        }
    }

    // Execute servo updates at 50Hz (20ms intervals)
    static unsigned long lastUpdate = 0;
    if (millis() - lastUpdate >= 20) {
        lastUpdate = millis();
        processSamerPayload(lastCommand, tiltServo, panServo);
    }
}