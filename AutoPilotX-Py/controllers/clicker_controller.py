import keyboard
from pynput.mouse import Button
from services.ClickerService import ClickerService

class ClickerController:
    def __init__(self, view):
        self.view = view
        self.service = None
        self.view.toggleButton.clicked.connect(self.toggle_clicker)
        self.view.hotkeyLineEdit.textChanged.connect(self.set_hotkey)

    def set_hotkey(self, hotkey):
        keyboard.add_hotkey(hotkey, self.toggle_clicker)

    def toggle_clicker(self):
        if self.service and self.service.running:
            self.service.stop()
            self.service = None
            self.view.toggleButton.setText("Start")
        else:
            click_type = self.view.clickTypeComboBox.currentText()
            interval = self.view.intervalSpinBox.value()
            position = None
            if self.view.customPositionRadioButton.isChecked():
                x = self.view.xSpinBox.value()
                y = self.view.ySpinBox.value()
                position = (x, y)

            button = Button.left
            if click_type == "Right":
                button = Button.right
            elif click_type == "Middle":
                button = Button.middle

            self.service = ClickerService(interval, button, position)
            self.service.start()
            self.view.toggleButton.setText("Stop")
