from PyQt6 import uic
from PyQt6.QtWidgets import QWidget
from controllers.settings_controller import SettingsController

class SettingsView(QWidget):
    def __init__(self):
        super().__init__()
        uic.loadUi("AutoPilotX-Py/ui/settings_view.ui", self)
        self.controller = SettingsController(self)
