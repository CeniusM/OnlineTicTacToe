import socket

from communicator import Communicator
from tictactoe import TicTacToe


def _get_server_address():
    return "192.168.68.62", 4123
    # ip_address = input("Enter server ip address:")
    # port = input("Enter server port number:")
    # return ip_address, port


def _get_socket(server_address) -> socket:
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    print('connecting to %s port %s...' % server_address, end='')
    sock.connect(server_address)
    print('ok')

    return sock


def run():
    server_address = _get_server_address()
    sock = _get_socket(server_address)

    try:
        communicator = Communicator(sock)

        ttt = TicTacToe(communicator)
        ttt.run()
    finally:
        print('closing socket...', end='')
        sock.close()
        print('ok')


if __name__ == '__main__':
    run()
