/* Ports and Pins
Direct port access is much faster than digitalWrite.
You must match the correct port and pin as shown in the table below.

Arduino Pin        Port        Pin
13 (SCK)           PORTB       5
12 (MISO)          PORTB       4
11 (MOSI)          PORTB       3
10 (SS)            PORTB       2
9                  PORTB       1
8                  PORTB       0
7                  PORTD       7
6                  PORTD       6
5                  PORTD       5
4                  PORTD       4
3                  PORTD       3
2                  PORTD       2
1 (TX)             PORTD       1
0 (RX)             PORTD       0
A5 (Analog)        PORTC       5
A4 (Analog)        PORTC       4
A3 (Analog)        PORTC       3
A2 (Analog)        PORTC       2
A1 (Analog)        PORTC       1
A0 (Analog)        PORTC       0
*/

// Defines for use with Arduino functions
#define clockpin   13 // CL
#define enablepin  10 // BL
#define latchpin    9 // XL
#define datapin    11 // SI

// Defines for direct port access
#define CLKPORT PORTB
#define ENAPORT PORTB
#define LATPORT PORTB
#define DATPORT PORTB
#define CLKPIN  5
#define ENAPIN  2
#define LATPIN  1
#define DATPIN  3

//
const byte START_FRAME_INDICATOR = 0;
const byte END_FRAME_INDICATOR = 255;

// Variables for communication
unsigned long SB_CommandPacket;
int SB_CommandMode;
int SB_BlueCommand;
int SB_RedCommand;
int SB_GreenCommand;

// Define number of ShiftBrite modules
#define NumLEDs 100

// Create LED value storage array
uint16_t LEDChannels[NumLEDs][3] = {0};

byte inByte = 0;
byte group = 0;

// RGB Color
int red = 0;
int green = 0;
int blue = 0;

//
unsigned long startTime = millis();
unsigned long currentTime = 0;
byte frames = 0;

//
void setup() {
// Set pins to outputs and initial states
pinMode(datapin, OUTPUT);
pinMode(latchpin, OUTPUT);
pinMode(enablepin, OUTPUT);
pinMode(clockpin, OUTPUT);
digitalWrite(latchpin, LOW);
digitalWrite(enablepin, LOW);
SPCR = (1<<SPE)|(1<<MSTR)|(0<<SPR1)|(0<<SPR0);

  // Initialize the Serial Port
Serial.begin(115200); //57600 or 115200

// Reset the LEDs
WriteLEDArray();
}

//
void loop() {
//
inByte = Serial.read();

//
if(Serial.available() > 0) {
//
if(inByte == END_FRAME_INDICATOR) {
//
WriteLEDArray();

//
frames++;
}

//
else if(inByte == START_FRAME_INDICATOR) {
group = NumLEDs - 1;
red = 0;
green = 0;
blue = 0;
}

//
else if(red == 0) {
red = inByte << 2;  
      }
      
      //
      else if(green == 0) {
        green = inByte << 2;
      }
      
      //
      else if(blue == 0) {
        blue = inByte << 2;
               
        // LEDChannels[LED GROUP][RGB 0-2] = Brightness 0-1024
        LEDChannels[group][0] = red;   // RED
        LEDChannels[group][1] = green; // GREEN
        LEDChannels[group][2] = blue;  // BLUE
        
        //
        group--;
        red = 0;
        green = 0;
        blue = 0;
      }
      
      Serial.write(group);
        Serial.write(red);
        Serial.write(green);
        Serial.write(blue);
    }
}

// Send all array values to chain
void WriteLEDArray() {
  // Write to PWM control registers
  SB_CommandMode = B00; 

  //
  for (int i = 0; i<NumLEDs; i=""
    SB_RedCommand = "LEDChannels"[i=""][0=""];   // RED
SB_GreenCommand = LEDChannels[i][1]; // GREEN
SB_BlueCommand = LEDChannels[i][2];  // BLUE
SB_SendPacket();
}

//
SB_Latch();

// Write to current control registers
SB_CommandMode = B01;

//
for (int z = 0; z < NumLEDs; z++) {
    SB_SendPacket();   
  }

  //
  SB_Latch();
}

// Load values into SPI register
void SB_SendPacket() {
  if (SB_CommandMode == B01) {
    SB_RedCommand = 127;
    SB_GreenCommand = 127;
    SB_BlueCommand = 127;
  }

  SPDR = SB_CommandMode << 6 | SB_BlueCommand >> 4;
  while(!(SPSR & (1<<SPIF
  SPDR = "SB_BlueCommand" << 4 | SB_RedCommand >> 6;
  while(!(SPSR & (1<<SPIF
  SPDR = "SB_RedCommand" << 2 | SB_GreenCommand >> 8;
  while(!(SPSR & (1<<SPIF
  SPDR = "SB_GreenCommand"
  while=""(!(SPSR="" & (1<<SPIF)));
}

// Latch values into PWM registers
void SB_Latch() {
delayMicroseconds(1);
LATPORT += (1 << LATPIN);
  LATPORT &= ~(1 << LATPIN);
}
    