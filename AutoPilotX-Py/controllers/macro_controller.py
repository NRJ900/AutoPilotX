import os
import keyboard
from services.MacroService import MacroService

class MacroController:
    def __init__(self, view):
        self.view = view
        self.service = MacroService()
        self.view.recordButton.clicked.connect(self.toggle_record)
        self.view.playButton.clicked.connect(self.play_macro)
        self.view.saveButton.clicked.connect(self.save_macro)
        self.view.macroListWidget.itemDoubleClicked.connect(self.play_macro)
        self.view.hotkeyLineEdit.textChanged.connect(self.set_hotkey)
        self.load_macros()

    def set_hotkey(self, hotkey):
        keyboard.add_hotkey(hotkey, self.play_macro)

    def toggle_record(self):
        if self.service.recording:
            self.service.stop_recording()
            self.view.recordButton.setText("Record")
        else:
            self.service.record()
            self.view.recordButton.setText("Stop")

    def play_macro(self):
        if not self.service.playing:
            selected_items = self.view.macroListWidget.selectedItems()
            if selected_items:
                macro_name = selected_items[0].text()
                events = self.service.load(macro_name)
                self.service.play(events)
            else:
                self.service.play(self.service.events)

    def save_macro(self):
        name = self.view.macroNameLineEdit.text()
        if name:
            self.service.save(name, self.service.events)
            self.view.macroListWidget.addItem(name)
            self.view.macroNameLineEdit.clear()

    def load_macros(self):
        if not os.path.exists("AutoPilotX-Py/data/saved_macros"):
            os.makedirs("AutoPilotX-Py/data/saved_macros")
        for file in os.listdir("AutoPilotX-Py/data/saved_macros"):
            if file.endswith(".json"):
                self.view.macroListWidget.addItem(file.replace(".json", ""))
