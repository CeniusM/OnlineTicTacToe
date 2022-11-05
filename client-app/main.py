import socket
from tictactoe import TicTacToe


def _get_server_address():
    return "192.168.68.62", 4123
    ip_address = input("Enter server ip address:")
    port = input("Enter server port number:")
    return ip_address, port


def _get_socket(server_address) -> socket:
    try:
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

        print('connecting to %s port %s...' % server_address, end='')
        sock.connect(server_address)
        print('ok')

        #sock.sendall(b"Hi player 1! Let's rock!")
        return sock
        #while True:
        #    data = sock.recv(1024)
        #    print('received "%s"' % data)
        #    #sock.sendall(next(sequence).to_bytes(1, 'big'))
        #    #amount_received += len(data)

    finally:
        pass
    #    print('closing socket...', end='')
    #    sock.close()
    #    print('ok')


def run():
    server_address = _get_server_address()
    sock = _get_socket(server_address)
    send = lambda pos: sock.sendall(pos.to_bytes(1, 'big'))
    receive = lambda: int.from_bytes(sock.recv(1), 'big')
    ttt = TicTacToe(send, receive)
    ttt.run()


if __name__ == '__main__':
    run()
