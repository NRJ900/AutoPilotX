from PyQt6 import uic
from PyQt6.QtWidgets import QWidget
from controllers.clicker_controller import ClickerController

class ClickerView(QWidget):
    def __init__(self):
        super().__init__()
        uic.loadUi("AutoPilotX-Py/ui/clicker_view.ui", self)
        self.controller = ClickerController(self)
