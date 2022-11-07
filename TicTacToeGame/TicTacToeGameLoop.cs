using TicTacToeEngine;

namespace TicTacToeGame;

internal class TicTacToeGameLoop
{
    private Action<int> SendMove;
    private Func<int> TryGetMove;
    private Action<string> SendMessage;
    private Func<(bool MessageReady, string Message)> TryGetMessage;

    private int ThisPlayer = 1;

    public bool IsRunning { get; private set; }

    private TicTacToe game = new TicTacToe();
    private int LastMessageSize = 0;

    /// <summary>
    /// SendMove will send an int from 0 to 8 to indicate a move.
    /// TryGetMove is expected to send back -1 if there is no moves to get, otherwise a number between 0 and 8.
    /// </summary>
    /// <param name="SendMove"></param>
    /// <param name="TryGetMove"></param>
    public TicTacToeGameLoop(
        Action<int> SendMove,
        Func<int> TryGetMove,
        Action<string> SendMessage,
        Func<(bool MessageReady, string Message)> TryGetMessage
        )
    {
        this.SendMove = SendMove;
        this.TryGetMove = TryGetMove;
        this.SendMessage = SendMessage;
        this.TryGetMessage = TryGetMessage;
        IsRunning = false;
    }

    /// <summary>
    /// Start the game loop.
    /// </summary>
    public void Start()
    {
        //FillRect('@', 10, 10, 10, 10);
        //FillRect('b', 10, 20, 10, 10);
        //FillRect('C', 10, 30, 20, 20);

        //SendMessage("HELP SOS IM FRICKED");

        if (IsRunning)
            return;
        IsRunning = true;
        game = new TicTacToe();
        //ThisPlayer = new Random().Next(1, 3);
        ThisPlayer = 1;
        GameLoop();
        IsRunning = false;
    }

    private void GameLoop()
    {
        while (true)
        {
            if (game.player == ThisPlayer)
            {
                // This player
                while (true)
                {
                    // Messages
                    var MabyMessage = TryGetMessage();
                    if (MabyMessage.MessageReady)
                    {
                        (int Left, int Right) poss = Console.GetCursorPosition();
                        FillRect(' ', 30, 0, LastMessageSize, 1);
                        LastMessageSize = MabyMessage.Message.Length;
                        Console.SetCursorPosition(30, 0);
                        Console.Write(MabyMessage.Message);
                        Console.SetCursorPosition(poss.Left, poss.Right);
                    }

                    // clear board
                    FillRect(' ', 0, 0, 20, 10);

                    PrintBoard(0, 0);

                    (int Left, int Right) pos = Console.GetCursorPosition();
                    Console.Write("x y");
                    Console.SetCursorPosition(pos.Left, pos.Right);
                    string Input = Console.ReadLine()!;
                    string[] Inputs = Input.Split(" ");

                    if (Inputs.Length != 2 || !int.TryParse(Inputs[0], out int foo0) || !int.TryParse(Inputs[1], out int foo1))
                    {
                        SendMessage(Input);
                        continue;
                    }

                    int x = int.Parse(Inputs[0]) - 1;
                    int y = 4 - int.Parse(Inputs[1]) - 1;
                    if (x > 2 || x < 0 || y > 2 || y < 0)
                        continue;
                    int move = x + (y * 3);

                    if (game.MakeMove(move))
                    {
                        SendMove(move);
                        break;
                    }
                }
            }
            else
            {
                // clear board
                FillRect(' ', 0, 0, 20, 10);

                // Oponnent
                PrintBoard(0, 0);

                int Waited = 0;
                while (game.player != ThisPlayer)
                {
                    // Messages
                    var MabyMessage = TryGetMessage();
                    if (MabyMessage.MessageReady)
                    {
                        (int Left, int Right) poss = Console.GetCursorPosition();
                        FillRect(' ', 30, 0, LastMessageSize, 1);
                        LastMessageSize = MabyMessage.Message.Length;
                        Console.SetCursorPosition(30, 0);
                        Console.Write(MabyMessage.Message);
                        Console.SetCursorPosition(poss.Left, poss.Right);
                    }


                    int move = TryGetMove();

                    if (move != -1)
                    {
                        if (move > 9 || move < 0)
                            continue;
                        game.MakeMove(move);
                        break;
                    }


                    (int Left, int Right) pos = Console.GetCursorPosition();
                    Console.Write("Waiting");
                    for (int i = 0; i < Waited; i++)
                    {
                        Console.Write(".");
                    }
                    Console.Write("       \n");
                    Console.SetCursorPosition(pos.Left, pos.Right);

                    Thread.Sleep(250);
                    Waited++;
                    if (Waited == 5)
                        Waited = 0;
                }
            }

            Thread.Sleep(20);
        }
    }

    private void PrintBoard(int x, int y)
    {
        Console.SetCursorPosition(x, y);

        Console.WriteLine("Player Turn: " + (game.player == 1 ? "You" : "Opponent") + "\n");
        for (int i = 0; i < 3; i++)
        {
            if (i == 0)
                Console.Write("^");
            else if (i == 1)
                Console.Write("|");
            else if (i == 2)
                Console.Write("y");

            for (int j = 0; j < 3; j++)
            {
                int piece = game.board[j + (i * 3)];
                if (piece == 0)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("[ ]");
                }
                else if (piece == 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[X]");
                }
                else if (piece == 2)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("[O]");
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.Write("\n");
        }
        Console.Write("  x-->\n");
    }

    public void FillRect(char value, int x, int y, int w, int h)
    {
        string Line = new string(value, w);

        for (int i = 0; i < h; i++)
        {
            Console.SetCursorPosition(x, y + i);
            Console.Write(Line);
        }
    }
}