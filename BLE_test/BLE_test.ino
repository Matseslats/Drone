#include <ArduinoBLE.h>

BLEService customService("19B10000-E8F2-537E-4F6C-D104768A1214"); // create a BLE service

BLEIntCharacteristic pitchCharacteristic("19B10001-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify); // create a BLE characteristic for pitch
BLEIntCharacteristic rollCharacteristic("19B10002-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify); // create a BLE characteristic for roll
BLEIntCharacteristic yawCharacteristic("19B10003-E8F2-537E-4F6C-D104768A1214", BLERead | BLENotify); // create a BLE characteristic for yaw
BLEBoolCharacteristic ledCharacteristic("19B10004-E8F2-537E-4F6C-D104768A1214", BLEWrite); // create a BLE characteristic for LED control

void setup() {
  Serial.begin(9600);

  // set up the BLE
  if (!BLE.begin()) {
    Serial.println("Starting BLE failed!");
    while (1);
  }

  // set the local name peripheral advertises
  BLE.setLocalName("PortentaBLE");
  BLE.setAdvertisedService(customService);

  // add the characteristics to the service
  customService.addCharacteristic(pitchCharacteristic);
  customService.addCharacteristic(rollCharacteristic);
  customService.addCharacteristic(yawCharacteristic);
  customService.addCharacteristic(ledCharacteristic);

  // add service
  BLE.addService(customService);

  // set the initial values for the characteristics
  pitchCharacteristic.setValue(3);
  rollCharacteristic.setValue(90);
  yawCharacteristic.setValue(5);
  ledCharacteristic.setValue(false);

  // start advertising
  BLE.advertise();
  Serial.println("Bluetooth device active, waiting for connections...");
}

void loop() {
  // wait for a BLE central
  BLEDevice central = BLE.central();

  // if a central is connected to the peripheral
  if (central) {
    Serial.print("Connected to central: ");
    Serial.println(central.address());

    // while the central is still connected
    while (central.connected()) {
      // if data is available to read
      if (ledCharacteristic.written()) {
        // read the LED state from the central
        bool ledState = ledCharacteristic.value();
        Serial.print("Received LED State: ");
        Serial.println(ledState);

        // perform action based on LED state (e.g., control an actual LED)
        digitalWrite(LED_BUILTIN, ledState ? HIGH : LOW);
      }

      // perform other tasks here
      delay(10);
    }

    // when the central disconnects, print it out
    Serial.print("Disconnected from central: ");
    Serial.println(central.address());
  }
}
