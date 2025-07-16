import json
from models.SettingsModel import SettingsModel

class SettingsController:
    def __init__(self, view):
        self.view = view
        self.model = SettingsModel()
        self.view.saveButton.clicked.connect(self.save_settings)
        self.view.loadButton.clicked.connect(self.load_settings)
        self.load_settings()

    def save_settings(self):
        self.model.start_with_windows = self.view.startWithWindowsCheckBox.isChecked()
        with open("AutoPilotX-Py/data/settings.json", "w") as f:
            json.dump(self.model.__dict__, f)

    def load_settings(self):
        try:
            with open("AutoPilotX-Py/data/settings.json", "r") as f:
                settings = json.load(f)
                self.model.start_with_windows = settings.get("start_with_windows", False)
                self.view.startWithWindowsCheckBox.setChecked(self.model.start_with_windows)
        except FileNotFoundError:
            pass
