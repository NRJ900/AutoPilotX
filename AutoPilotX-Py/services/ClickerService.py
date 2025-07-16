import time
import threading
from pynput.mouse import Button, Controller

class ClickerService(threading.Thread):
    def __init__(self, interval, button, position=None):
        super().__init__()
        self.interval = interval
        self.button = button
        self.position = position
        self.running = False
        self.mouse = Controller()

    def run(self):
        self.running = True
        while self.running:
            if self.position:
                self.mouse.position = self.position
            self.mouse.click(self.button)
            time.sleep(self.interval / 1000)

    def stop(self):
        self.running = False
