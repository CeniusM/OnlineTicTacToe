import socket


class Communicator:

    def __init__(self, sock: socket):
        self.socket = sock

    def send_position(self, pos: int):
        b = pos.to_bytes(1, 'big')
        self.socket.sendall(b)

    def receive_position(self) -> int:
        b = self.socket.recv(1024)
        position = int.from_bytes(b, 'big')

        if position < 0 or position > 8:
            raise Exception(f"Received invalid position {position} from remote player.")

        return position
