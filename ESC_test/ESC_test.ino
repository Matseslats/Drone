#include <Servo.h>

byte servoPin = D0;
Servo servo;

void setup() {
  
 Serial.begin(9600);
 servo.attach(servoPin);

 servo.writeMicroseconds(1510); // send "stop" signal to ESC.

 delay(7000); // delay to allow the ESC to recognize the stopped signal
}

void loop() {
  
  Serial.println("Enter PWM signal value 1100 to 1900, 1500 to stop");
  
  while (Serial.available() == 0);
  
  int val = Serial.parseInt(); 
  
  if(val < 1300 || val > 1700) // Goes grom 1100 to 1900, but that is very powerfull!!
  {
    Serial.println("not valid");
  }
  else
  {
    Serial.print("New value: ");
    Serial.println(val);
    servo.writeMicroseconds(val); // Send signal to ESC.
  }
}