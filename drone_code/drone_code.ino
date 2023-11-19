#include <Adafruit_ICM20X.h>
#include <Adafruit_ICM20948.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>

#include <ArduinoBLE.h>

#include "SensorFusion.h" //SF
SF fusion;

BLEService dataServiceBLE("19B10000-E8F2-537E-4F6C-D104768A1214"); // create a BLE service

BLEFloatCharacteristic      pitchCharacteristic("19B10001-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify); // create a BLE characteristic for pitch
BLEDescriptor              pitchLabelDescriptor("19B10A01-E8F2-537E-4F6C-D104768A1214", "Pitch");
BLEFloatCharacteristic       rollCharacteristic("19B10002-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify); // create a BLE characteristic for roll
BLEDescriptor               rollLabelDescriptor("19B10A02-E8F2-537E-4F6C-D104768A1214", "Roll");
BLEFloatCharacteristic        yawCharacteristic("19B10003-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify); // create a BLE characteristic for yaw
BLEDescriptor                yawLabelDescriptor("19B10A03-E8F2-537E-4F6C-D104768A1214", "Yaw");

BLEBoolCharacteristic         ledCharacteristic("19B10004-E8F2-537E-4F6C-D104768A1214", BLEWrite); // create a BLE characteristic for LED control
BLEDescriptor                ledLabelDescriptor("19B10A04-E8F2-537E-4F6C-D104768A1214", "Green LED");

BLEDoubleCharacteristic  latitudeCharacteristic("19B10005-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify); // create a BLE characteristic for latitude
BLEDescriptor           latitudeLabelDescriptor("19B10A05-E8F2-537E-4F6C-D104768A1214", "Latitude");
BLEDoubleCharacteristic longitudeCharacteristic("19B10006-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify); // create a BLE characteristic for longitude
BLEDescriptor          longitudeLabelDescriptor("19B10A06-E8F2-537E-4F6C-D104768A1214", "Longitude");
BLEFloatCharacteristic   altitudeCharacteristic("19B10007-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify); // create a BLE characteristic for altitude
BLEDescriptor           altitudeLabelDescriptor("19B10A07-E8F2-537E-4F6C-D104768A1214", "Altitude");

BLEFloatCharacteristic   batteryLevelCharacteristic("19B10008-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify); // create a BLE characteristic for batteryLevel
BLEDescriptor           batteryLevelLabelDescriptor("19B10A08-E8F2-537E-4F6C-D104768A1214", "Battery Level");

BLEDevice central;

float gx, gy, gz, ax, ay, az, mx, my, mz;
float pitch, roll, yaw;
float deltat;
float batLevel;

#define ACCEL_RANGE_G 16
#define GYRO_RANGE_DPS 2000

#define LORA_INT 6
#define LORA_CS 17
#define LORA_RST 18
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

double latitude = 0, longitude = 0;
float altitude = 0;

void setup(void) {
  Serial.begin(9600);
  while (!Serial)
    delay(10); // will pause Zero, Leonardo, etc until serial console opens
  

  // set up the BLE
  if (!BLE.begin()) {
    Serial.println("Starting BLE failed!");
    while (1);
  }
  // set the local name peripheral advertises
  BLE.setLocalName("Portenta-Hopper-Drone");
  BLE.setAdvertisedService(dataServiceBLE);
  
  // add the characteristics to the service
  dataServiceBLE.addCharacteristic(pitchCharacteristic);
  pitchCharacteristic.addDescriptor(pitchLabelDescriptor);
  dataServiceBLE.addCharacteristic(rollCharacteristic);
  rollCharacteristic.addDescriptor(rollLabelDescriptor);
  dataServiceBLE.addCharacteristic(yawCharacteristic);
  yawCharacteristic.addDescriptor(yawLabelDescriptor);


  dataServiceBLE.addCharacteristic(ledCharacteristic);
  ledCharacteristic.addDescriptor(ledLabelDescriptor);


  dataServiceBLE.addCharacteristic(latitudeCharacteristic);
  latitudeCharacteristic.addDescriptor(latitudeLabelDescriptor);
  dataServiceBLE.addCharacteristic(longitudeCharacteristic);
  longitudeCharacteristic.addDescriptor(longitudeLabelDescriptor);
  dataServiceBLE.addCharacteristic(altitudeCharacteristic);
  altitudeCharacteristic.addDescriptor(altitudeLabelDescriptor);

  dataServiceBLE.addCharacteristic(batteryLevelCharacteristic);
  batteryLevelCharacteristic.addDescriptor(batteryLevelLabelDescriptor);

  // add service
  BLE.addService(dataServiceBLE);

  // set the initial values for the characteristics
  pitchCharacteristic.setValue(0);
  rollCharacteristic.setValue(0);
  yawCharacteristic.setValue(0);
  ledCharacteristic.setValue(false);
  latitudeCharacteristic.setValue(0);
  longitudeCharacteristic.setValue(0);

  // start advertising
  BLE.advertise();
  Serial.println("Bluetooth device active, ready for connections...");

  // Try to initialize!
  while (!icm.begin_I2C()) {
    Serial.println("Failed to find ICM20948 chip. Retyring");
    delay(10);
  }
  Serial.println("ICM20948 Found!");

  setup_imu();
}

void onPulse(){
  Serial.println("Pulse in interrupt!");
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
  static int loop_no = 0;

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

  // // Serial.print("Pitch:"); 
  // Serial.print(pitch);
  // Serial.print(",");
  // // Serial.print("Roll:"); 
  // Serial.print(roll);
  // Serial.print(",");
  // // Serial.print("Yaw:"); 
  // Serial.print(yaw);
  // Serial.println(";");

  if (loop_no % 25 == 0){
    handleBluetooth();
  }
  loop_no ++;
}

void handleBluetooth(){
  // Check if there is a central device connected
  if (!central) {
    central = BLE.central(); // Try to connect to a central device
    if (central) {
      Serial.print("Connected to central: ");
      Serial.println(central.address());
      // Perform actions when a new connection is established
      // For example, update Bluetooth values or send initial data
    }
  }
  if (central) {
    // Serial.print("Connected to central: ");
    // Serial.println(central.address());

    // while the central is still connected
    if (central.connected()) {
      digitalWrite(LEDB, LOW);

      // if data is available to read
      if (ledCharacteristic.written()) {
        // read the LED state from the central
        bool ledState = ledCharacteristic.value();
        // perform action based on LED state (e.g., control an actual LED)
        digitalWrite(LED_BUILTIN, ledState ? HIGH : LOW);
      }

      // Update values
      pitchCharacteristic.writeValue(pitch);
      rollCharacteristic.writeValue(roll);
      yawCharacteristic.writeValue(yaw);

      latitudeCharacteristic.writeValue(latitude);
      longitudeCharacteristic.writeValue(longitude);
      altitudeCharacteristic.writeValue(altitude);

      batteryLevelCharacteristic.writeValue(batLevel);
    } else {
      digitalWrite(LEDB, HIGH);
    }
  } else {
    digitalWrite(LEDB, HIGH);
  }
}