import sys
from PyQt6 import uic
from PyQt6.QtGui import QIcon, QAction
from PyQt6.QtWidgets import QMainWindow, QApplication, QSystemTrayIcon, QMenu
from views.clicker_view import ClickerView
from views.key_presser_view import KeyPresserView
from views.macro_view import MacroView
from views.settings_view import SettingsView

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        uic.loadUi("AutoPilotX-Py/ui/main_window.ui", self)
        self.dark_theme = True
        self.load_theme()
        self.actionTheme.triggered.connect(self.toggle_theme)

        self.clicker_view = ClickerView()
        self.key_presser_view = KeyPresserView()
        self.macro_view = MacroView()
        self.settings_view = SettingsView()
        self.stackedWidget.addWidget(self.clicker_view)
        self.stackedWidget.addWidget(self.key_presser_view)
        self.stackedWidget.addWidget(self.macro_view)
        self.stackedWidget.addWidget(self.settings_view)

        self.listWidget.addItem("Auto Clicker")
        self.listWidget.addItem("Auto Key Presser")
        self.listWidget.addItem("Macro Recorder")
        self.listWidget.addItem("Settings")
        self.listWidget.currentRowChanged.connect(self.stackedWidget.setCurrentIndex)

        self.tray_icon = QSystemTrayIcon(self)
        self.tray_icon.setIcon(QIcon("AutoPilotX-Py/resources/icon.png"))
        self.tray_icon.setToolTip("AutoPilotX")

        show_action = QAction("Show", self)
        quit_action = QAction("Exit", self)
        show_action.triggered.connect(self.show)
        quit_action.triggered.connect(QApplication.instance().quit)
        tray_menu = QMenu()
        tray_menu.addAction(show_action)
        tray_menu.addAction(quit_action)
        self.tray_icon.setContextMenu(tray_menu)
        self.tray_icon.show()


    def load_theme(self):
        if self.dark_theme:
            with open("AutoPilotX-Py/ui/style.qss", "r") as f:
                self.setStyleSheet(f.read())
        else:
            self.setStyleSheet("")

    def toggle_theme(self):
        self.dark_theme = not self.dark_theme
        self.load_theme()

if __name__ == '__main__':
    app = QApplication(sys.argv)
    window = MainWindow()
    window.show()
    sys.exit(app.exec())
