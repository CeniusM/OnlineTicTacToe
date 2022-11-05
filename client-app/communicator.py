import socket
import sys
from threading import Thread
import time
from logutils import debug, warn


class Communicator:

    def _poller(self):
        while True:
            bs = self.socket.recv(1024)
            if len(bs) == 0:
                time.sleep(1)
                continue
            debug(f">>>> {str(bs)}")
            code = bs[0]
            if code == 1:
                if len(bs) != 2:
                    warn("Expected two bytes (instruction and position) from remote player. Received only one.")
                    continue
                position = bs[1]
                if position < 0 or position > 8:
                    warn(f"Received invalid position {position} from remote player.")
                    continue
                self.next_received_position = position
            elif code == 2:
                if len(bs) < 5:
                    warn("Expected at least 5 bytes (1 byte instruction and int (4 byte little endian) message length) from remote player. Received only one.")
                    continue
                length = int.from_bytes(bs[1:4], 'little')
                message = bs[5:].decode("utf-8")
                debug(f"{message}")
                if len(message) != length:
                    warn(f"Expected remote message with length {length}, but received message with length {len(message)}")
                print("Remote player greets you:")
                print(f"> {message}")
            else:
                warn(f"Received invalid code {code} from remote player.")

    def __init__(self, sock: socket):
        self.next_received_position = None
        self.socket = sock
        thread = Thread(target=self._poller)
        thread.start()

    def send_position(self, pos: int):
        code_byte = int(1).to_bytes(1, 'big')
        pos_byte = pos.to_bytes(1, 'big')
        bs = code_byte + pos_byte
        debug(f"<<<<<<<<<< {bs}")
        self.socket.sendall(bs)

    def receive_position(self) -> int:
        while self.next_received_position is None:
            time.sleep(1)
        position = self.next_received_position
        self.next_received_position = None
        return position

    def send_message(self, message: str):
        bs = int(2).to_bytes(1, 'little') + len(message).to_bytes(4, 'little') + message.encode('utf-8')
        debug(f"Sending message: '{bs}'")
        self.socket.sendall(bs)
