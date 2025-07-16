import time
import threading
from pynput.keyboard import Controller

class KeyService(threading.Thread):
    def __init__(self, key_sequence, delay, loop, loop_count):
        super().__init__()
        self.key_sequence = key_sequence
        self.delay = delay
        self.loop = loop
        self.loop_count = loop_count
        self.running = False
        self.keyboard = Controller()

    def run(self):
        self.running = True
        count = 0
        while self.running:
            for key in self.key_sequence:
                self.keyboard.press(key)
                self.keyboard.release(key)
                time.sleep(self.delay / 1000)

            if self.loop == "Once":
                self.running = False
            elif self.loop == "N Times":
                count += 1
                if count >= self.loop_count:
                    self.running = False

    def stop(self):
        self.running = False
