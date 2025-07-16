import time
import json
import threading
from pynput import mouse, keyboard

class MacroService:
    def __init__(self):
        self.recording = False
        self.playing = False
        self.events = []
        self.mouse_listener = None
        self.keyboard_listener = None

    def record(self):
        self.recording = True
        self.events = []
        self.mouse_listener = mouse.Listener(on_click=self.on_click, on_scroll=self.on_scroll)
        self.keyboard_listener = keyboard.Listener(on_press=self.on_press, on_release=self.on_release)
        self.mouse_listener.start()
        self.keyboard_listener.start()

    def stop_recording(self):
        if self.recording:
            self.recording = False
            self.mouse_listener.stop()
            self.keyboard_listener.stop()

    def on_click(self, x, y, button, pressed):
        self.events.append({
            'type': 'click',
            'x': x,
            'y': y,
            'button': str(button),
            'pressed': pressed,
            'time': time.time()
        })

    def on_scroll(self, x, y, dx, dy):
        self.events.append({
            'type': 'scroll',
            'x': x,
            'y': y,
            'dx': dx,
            'dy': dy,
            'time': time.time()
        })

    def on_press(self, key):
        self.events.append({
            'type': 'press',
            'key': str(key),
            'time': time.time()
        })

    def on_release(self, key):
        self.events.append({
            'type': 'release',
            'key': str(key),
            'time': time.time()
        })

    def play(self, events):
        self.playing = True
        mouse_controller = mouse.Controller()
        keyboard_controller = keyboard.Controller()

        for i, event in enumerate(events):
            if not self.playing:
                break

            if i > 0:
                time.sleep(event['time'] - events[i-1]['time'])

            if event['type'] == 'click':
                mouse_controller.position = (event['x'], event['y'])
                button = eval(event['button'].replace('Button.', 'mouse.Button.'))
                if event['pressed']:
                    mouse_controller.press(button)
                else:
                    mouse_controller.release(button)
            elif event['type'] == 'scroll':
                mouse_controller.position = (event['x'], event['y'])
                mouse_controller.scroll(event['dx'], event['dy'])
            elif event['type'] == 'press':
                key = self.parse_key(event['key'])
                keyboard_controller.press(key)
            elif event['type'] == 'release':
                key = self.parse_key(event['key'])
                keyboard_controller.release(key)

        self.playing = False

    def stop_playing(self):
        self.playing = False

    def save(self, name, events):
        with open(f"AutoPilotX-Py/data/saved_macros/{name}.json", "w") as f:
            json.dump(events, f)

    def load(self, name):
        with open(f"AutoPilotX-Py/data/saved_macros/{name}.json", "r") as f:
            return json.load(f)

    def parse_key(self, key_str):
        if key_str.startswith('Key.'):
            return eval(key_str.replace('Key.', 'keyboard.Key.'))
        else:
            return key_str.replace("'", "")
