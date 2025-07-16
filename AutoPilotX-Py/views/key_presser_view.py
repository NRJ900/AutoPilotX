from PyQt6 import uic
from PyQt6.QtWidgets import QWidget
from controllers.key_presser_controller import KeyPresserController

class KeyPresserView(QWidget):
    def __init__(self):
        super().__init__()
        uic.loadUi("AutoPilotX-Py/ui/key_presser_view.ui", self)
        self.controller = KeyPresserController(self)
