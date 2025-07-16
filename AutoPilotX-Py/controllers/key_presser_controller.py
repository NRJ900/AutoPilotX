import keyboard
from services.KeyService import KeyService

class KeyPresserController:
    def __init__(self, view):
        self.view = view
        self.service = None
        self.view.toggleButton.clicked.connect(self.toggle_key_presser)
        self.view.loopComboBox.currentIndexChanged.connect(self.toggle_loop_count)
        self.view.hotkeyLineEdit.textChanged.connect(self.set_hotkey)

    def set_hotkey(self, hotkey):
        keyboard.add_hotkey(hotkey, self.toggle_key_presser)

    def toggle_key_presser(self):
        if self.service and self.service.running:
            self.service.stop()
            self.service = None
            self.view.toggleButton.setText("Start")
        else:
            key_sequence = self.view.keySequenceLineEdit.text()
            delay = self.view.delaySpinBox.value()
            loop = self.view.loopComboBox.currentText()
            loop_count = self.view.loopCountSpinBox.value()

            self.service = KeyService(key_sequence, delay, loop, loop_count)
            self.service.start()
            self.view.toggleButton.setText("Stop")

    def toggle_loop_count(self, index):
        if self.view.loopComboBox.currentText() == "N Times":
            self.view.loopCountSpinBox.setEnabled(True)
        else:
            self.view.loopCountSpinBox.setEnabled(False)
