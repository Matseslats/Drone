#include <Adafruit_ICM20X.h>
#include <Adafruit_ICM20948.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>

#include "SensorFusion.h" //SF
SF fusion;

float gx, gy, gz, ax, ay, az, mx, my, mz;
float pitch, roll, yaw;
float deltat;

#define ACCEL_RANGE_G 16
#define GYRO_RANGE_DPS 2000

#define LORA_CS A2
#define LORA_RST A3
#define BAT_LEVEL A4

#define ESC1 0
#define ESC2 1
#define ESC3 2
#define ESC4 3

#define SERVO1 4
#define SERVO2 5
#define SERVO3 6

#define SPI_CS 7
#define SPI_MOSI 8
#define SPI_CK 9
#define SPI_MISO 10

#define I2C_SDA 11
#define I2C_SCL 12

#define UART_RX 13
#define UART_TX 14

#define ICM_CS SPI_CS
// For software-SPI mode we need SCK/MOSI/MISO pins
#define ICM_SCK SPI_CK
#define ICM_MISO SPI_MISO
#define ICM_MOSI SPI_MOSI

uint16_t measurement_delay_us = 65535; // IMU delay between measurements for testing
Adafruit_ICM20948 icm;

// #include <Servo.h> 

//   Servo esc1;
//   Servo esc2;
//   Servo esc3;
//   Servo esc4;

void setup(void) {
  Serial.begin(9600);
  while (!Serial)
    delay(10); // will pause Zero, Leonardo, etc until serial console opens

  Serial.println("Adafruit ICM20948 test!");

  // Try to initialize!
  if (!icm.begin_I2C()) {

    Serial.println("Failed to find ICM20948 chip");
    while (1) {
      delay(10);
    }
  }
  Serial.println("ICM20948 Found!");

  setup_imu();
}

void setup_imu(){
  // icm.setAccelRange(ICM20948_ACCEL_RANGE_16_G);
  Serial.print("Accelerometer range set to: ");
  switch (icm.getAccelRange()) {
  case ICM20948_ACCEL_RANGE_2_G:
    Serial.println("+-2G");
    break;
  case ICM20948_ACCEL_RANGE_4_G:
    Serial.println("+-4G");
    break;
  case ICM20948_ACCEL_RANGE_8_G:
    Serial.println("+-8G");
    break;
  case ICM20948_ACCEL_RANGE_16_G:
    Serial.println("+-16G");
    break;
  }
  Serial.println("OK");

  // icm.setGyroRange(ICM20948_GYRO_RANGE_2000_DPS);
  Serial.print("Gyro range set to: ");
  switch (icm.getGyroRange()) {
  case ICM20948_GYRO_RANGE_250_DPS:
    Serial.println("250 degrees/s");
    break;
  case ICM20948_GYRO_RANGE_500_DPS:
    Serial.println("500 degrees/s");
    break;
  case ICM20948_GYRO_RANGE_1000_DPS:
    Serial.println("1000 degrees/s");
    break;
  case ICM20948_GYRO_RANGE_2000_DPS:
    Serial.println("2000 degrees/s");
    break;
  }

  icm.setAccelRateDivisor(10);
  uint16_t accel_divisor = icm.getAccelRateDivisor();
  float accel_rate = 1125 / (1.0 + accel_divisor);

  Serial.print("Accelerometer data rate divisor set to: ");
  Serial.println(accel_divisor);
  Serial.print("Accelerometer data rate (Hz) is approximately: ");
  Serial.println(accel_rate);

  //  icm.setGyroRateDivisor(255);
  uint8_t gyro_divisor = icm.getGyroRateDivisor();
  float gyro_rate = 1100 / (1.0 + gyro_divisor);

  Serial.print("Gyro data rate divisor set to: ");
  Serial.println(gyro_divisor);
  Serial.print("Gyro data rate (Hz) is approximately: ");
  Serial.println(gyro_rate);

  // icm.setMagDataRate(AK09916_MAG_DATARATE_10_HZ);
  Serial.print("Magnetometer data rate set to: ");
  switch (icm.getMagDataRate()) {
  case AK09916_MAG_DATARATE_SHUTDOWN:
    Serial.println("Shutdown");
    break;
  case AK09916_MAG_DATARATE_SINGLE:
    Serial.println("Single/One shot");
    break;
  case AK09916_MAG_DATARATE_10_HZ:
    Serial.println("10 Hz");
    break;
  case AK09916_MAG_DATARATE_20_HZ:
    Serial.println("20 Hz");
    break;
  case AK09916_MAG_DATARATE_50_HZ:
    Serial.println("50 Hz");
    break;
  case AK09916_MAG_DATARATE_100_HZ:
    Serial.println("100 Hz");
    break;
  }
  Serial.println();
}

void loop() {

  // //  /* Get a new normalized sensor event */
  sensors_event_t accel;
  sensors_event_t gyro;
  sensors_event_t mag;
  sensors_event_t temp;

  icm.getEvent(&accel, &gyro, &temp, &mag);

  deltat = fusion.deltatUpdate(); //this have to be done before calling the fusion update
  //choose only one of these two:
  // fusion.MahonyUpdate(
  //   gyro.gyro.x * DEG_TO_RAD, gyro.gyro.y * DEG_TO_RAD, gyro.gyro.z * DEG_TO_RAD, 
  //   accel.acceleration.x, accel.acceleration.y, accel.acceleration.z, 
  //   deltat);  //mahony is suggested if there isn't the mag and the mcu is slow
  fusion.MadgwickUpdate(gyro.gyro.x, gyro.gyro.y, gyro.gyro.z, 
    accel.acceleration.x, accel.acceleration.y, accel.acceleration.z, 
    mag.magnetic.x, mag.magnetic.y, mag.magnetic.z, 
    deltat);  //else use the magwick, it is slower but more accurate

  pitch = fusion.getPitch();
  roll = fusion.getRoll();    //you could also use getRollRadians() ecc
  yaw = fusion.getYaw();

  // Serial.print("Pitch:"); 
  Serial.print(pitch);
  Serial.print(",");
  // Serial.print("Roll:"); 
  Serial.print(roll);
  Serial.print(",");
  // Serial.print("Yaw:"); 
  Serial.print(yaw);
  Serial.println(";");

}
