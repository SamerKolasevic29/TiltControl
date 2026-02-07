#include <Arduino.h>
#include <WiFi.h>
#include <WiFiUdp.h>
#include <ESP32Servo.h>
#include "SamerProtocol.h"

//config 
const char* ssid = "žnj";
const char* password = "ovvoonno";
const unsigned int localUdpPort = 4210;
const int tiltPin = 18;
const int panPin = 19;
const int statusLed = 2;

WiFiUDP udp;
Servo tiltServo;
Servo panServo;
char incomingPacket[255];

String lastCommand = "STOPSTOP";
unsigned long lastPacketTime = 0;
const unsigned long TIMEOUT_MS = 300;  

void setup() {
    Serial.begin(115200);
    pinMode(statusLed, OUTPUT);
    
    // Servo init
    tiltServo.setPeriodHertz(50);
    tiltServo.attach(tiltPin, 500, 2500);
    panServo.setPeriodHertz(50);
    panServo.attach(panPin, 500, 2500);
    
    Serial.println("\n╔═══════════════════════════════╗");
    Serial.println("║   LUMINA PAN-TILT SISTEM     ║");
    Serial.println("╚═══════════════════════════════╝");
    
    // Calibrate on startup
    tiltServo.write(90);
    panServo.write(90);
    Serial.println("✓ Servos calibrated to 90°");
    
    // WiFi
    Serial.print("Connecting to WiFi");
    WiFi.begin(ssid, password);
    
    while (WiFi.status() != WL_CONNECTED) {
        digitalWrite(statusLed, !digitalRead(statusLed));
        delay(250);
        Serial.print(".");
    }
    
    digitalWrite(statusLed, HIGH);
    Serial.println("\n✓ WiFi connected!");
    Serial.print("IP: ");
    Serial.println(WiFi.localIP());
    
    udp.begin(localUdpPort);
    Serial.printf("✓ UDP listening on port %d\n", localUdpPort);
    Serial.println("═══════════════════════════════");
}

void loop() {
    unsigned long now = millis();
    static unsigned long lastServoUpdate = 0;

    // 1. read network packages
    int packetSize = udp.parsePacket();
    if (packetSize) {
        int len = udp.read(incomingPacket, 255);
        if (len > 0) {
            incomingPacket[len] = 0;
            lastCommand = String(incomingPacket);
        }
    }

    // 2. execute every 20ms
    if (now - lastServoUpdate >= 20) {
        lastServoUpdate = now;
        processSamerPayload(lastCommand, tiltServo, panServo);
    }
}