from PyQt6 import uic
from PyQt6.QtWidgets import QWidget
from controllers.macro_controller import MacroController

class MacroView(QWidget):
    def __init__(self):
        super().__init__()
        uic.loadUi("AutoPilotX-Py/ui/macro_view.ui", self)
        self.controller = MacroController(self)
