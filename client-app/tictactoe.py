class TicTacToe:

    def __init__(self, send, receive):
        self.send = send
        self.receive = receive
        self.board = ["_" for i in range(0, 10)]

    def run(self):
        while True:
            self._display()
            move = self.receive()
            self.board[move] = "X"
            self._display()
            move = int(input("Select field: "))
            self.board[move] = "O"
            self.send(move)

    def _display(self):
        for i in range(9):
            print(self.board[i], end="")
            if (i+1) % 3 == 0:
                print()
