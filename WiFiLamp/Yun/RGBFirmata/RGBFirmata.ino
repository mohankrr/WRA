/*
* Sketch for Arduino Yun to change color of RGB LED strip.
* Color is recieved from any Firmata Client as a string.
* Created by Mohan Palanisamy on Dec 24, 2015 as a simple sketch to  
* use with Windows Remote Arduino library
* Sketch should work with any Firmata Client
 */
#include <Firmata.h>
#include "FastLED.h"

#define NUM_LEDS 10
#define DATA_PIN 3

// Define the array of leds
CRGB leds[NUM_LEDS];

void stringCallback(char *myString)
{
  String rgbString=String(myString);
  int rIndex=rgbString.indexOf(',');
  int gIndex=rgbString.indexOf(',', rIndex+1);

  int r= rgbString.substring(0,rIndex).toInt();
  int g= rgbString.substring(rIndex+1,gIndex).toInt();
  int b= rgbString.substring(gIndex+1).toInt();
  
  for(int i=0;i<NUM_LEDS;i++)
  {
    leds[i]=CRGB(r,g,b);
    FastLED.show();
  }
}

void setup()
{
  Firmata.setFirmwareVersion(FIRMATA_MAJOR_VERSION, FIRMATA_MINOR_VERSION);
  
  //Interested only in the String_DATA callback. Ignore all other
  Firmata.attach(STRING_DATA, stringCallback);

  //This is Yun Specific.. Connects to Serail1 avaialble in Yun
  Serial1.begin(115200); 
  //this code is to delay the firmata begin process to avoid interference with the boot process when arduino linux side is restarted.
    do {
     while (Serial1.available() > 0) {
        Serial1.read();
        }
    delay(1000);
  } while (Serial1.available()>0);
  
  Firmata.begin(Serial1);

  FastLED.addLeds<TM1803, DATA_PIN, RBG>(leds, NUM_LEDS);

}

void loop()
{
  while (Firmata.available()) {
    Firmata.processInput();
  }
}


