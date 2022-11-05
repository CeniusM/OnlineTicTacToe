import socket
import sys


def _get_server_address():
    return "192.168.68.62", 4123
    ip_address = input("Enter server ip address:")
    port = input("Enter server port number:")
    return ip_address, port


sequence = iter(bytes("hej med dig cenius, hvordan gaar det i din ende?", 'ascii'))


def _loop(server_address):
    try:
        # Create a TCP/IP socket
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

        # Connect the socket to the port where the server is listening
        # server_address = ('localhost', 10000)

        print('connecting to %s port %s...' % server_address, end='')
        sock.connect(server_address)
        print('ok')

        sock.sendall(b"Hi Cenius! Let's rock!")

        while True:
            data = sock.recv(1024)
            print('received "%s"' % data)
            sock.sendall(next(sequence).to_bytes(1, 'big'))
            #amount_received += len(data)

    finally:
        print('closing socket...', end='')
        sock.close()
        print('ok')


def run():
    server_address = _get_server_address()
    _loop(server_address)


# Press the green button in the gutter to run the script.
if __name__ == '__main__':
    run()
