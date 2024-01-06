const int numPins = 54;
int lastButtonState[numPins];
bool serialInit = false;

void setup() {
  for (int i = 2; i < numPins; i++) {
    pinMode(i, INPUT_PULLUP);
  }
  Serial.begin(9600);
}

void loop() {
  if (serialInit) {
    checkInput();
  }

  if (Serial.available() > 0) {
    processSerialData();
  }
}

void checkInput() {
  for (int i = 2; i < numPins; i++) {
    int buttonState = digitalRead(i);

    if (buttonState != lastButtonState[i]) {
      lastButtonState[i] = buttonState;
      Serial.println("SW:" + String(i) + ":" + String(buttonState == LOW));
    }
  }
}

void processSerialData() {
  String data = Serial.readStringUntil('\n');
  data.trim();

  if (serialInit && data == "START") {
    resetButtonStates();
  }

  if (serialInit) {
    setPinOutput(data);
  }

  if (!serialInit && data == "START") {
    startSerialCommunication(data);
  }
}

void resetButtonStates() {
  serialInit = false;
  for (int i = 0; i < numPins; i++) {
    lastButtonState[i] = 2;
  }
}

void setPinOutput(String data) {
  int index1 = data.indexOf(':');
  int index2 = data.indexOf(':', index1 + 1);

  int pin = data.substring(index1 + 1, index2).toInt();
  int value = data.substring(index2 + 1).toInt();

  pinMode(pin, OUTPUT);

  digitalWrite(pin, value == HIGH ? HIGH : LOW);
}

void startSerialCommunication(String data) {
  serialInit = true;
  Serial.println(data);
}