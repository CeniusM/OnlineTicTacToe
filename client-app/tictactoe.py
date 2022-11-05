from communicator import Communicator


class TicTacToe:

    def __init__(self, communicator: Communicator):
        self.communicator = communicator
        self.board = ["_" for _ in range(0, 10)]

    def run(self):
        while True:
            self._display()
            move = self.communicator.receive_position()
            if self.board[move] != "_":
                raise Exception(f"Received invalid move at position {move} by remote player.")
            self.board[move] = "X"
            self._display()

            while True:
                msg = input("Select field: ")
                if msg.isdecimal():
                    move = int(msg)
                    break
                else:
                    self.communicator.send_message(msg)

            self.board[move] = "O"
            self.communicator.send_position(move)

    def _display(self):
        for i in range(9):
            print(self.board[i], end="")
            if (i+1) % 3 == 0:
                print()
        print()
