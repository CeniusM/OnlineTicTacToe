using TicTacToeEngine;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;

namespace TicTacToeGame;

internal class Game
{
    bool GameRunning = false;

    Func<bool> HasWifi = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable;

    const int PortUsed = 4123;
    string LocalIP = "\0";

    // Used by other side
    //Client = new TcpClient(new IPEndPoint(IPAddress.Parse(LocalIP), PortUsed));
    TcpClient? Client = null;
    NetworkStream? NetworkStream = null;
    ConcurrentQueue<byte> Data = new ConcurrentQueue<byte>();
    TcpListener? tcpListener = null;

    NetworkStream? OpponentDataStream = null;

    TicTacToeGameLoop gameLoop;

    public Game()
    {
        gameLoop = new TicTacToeGameLoop(SendMove, TryGetMove);
    }

    public void Start()
    {
        try
        {
            LocalIP = Helper.GetLocalIPAddressString();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.ReadLine();
            return;
        }

        if (!HasWifi())
        {
            Console.WriteLine("You are currently not online");
            Console.ReadLine();
            return;
        }

        GameRunning = true;

        EstablishConnectionWithOpponent();

        gameLoop.Start();

        GameRunning = false;
    }

    private void EstablishConnectionWithOpponent()
    {
        if (NetworkStream is not null)
        {
            NetworkStream.Dispose();
            NetworkStream = null;
        }
        if (Client is not null)
        {
            Client.Dispose();
            Client = null;
        }
        if (OpponentDataStream is not null)
        {
            OpponentDataStream.Dispose();
            OpponentDataStream = null;
        }
        if (tcpListener is not null)
        {
            tcpListener.Stop();
            tcpListener = null;
        }

        tcpListener = new TcpListener(IPAddress.Parse(LocalIP), PortUsed);

        tcpListener.Start();

        Task ConnectTask = new Task(async () =>
        {
            Client = await tcpListener.AcceptTcpClientAsync();
        }
        );
        ConnectTask.Start();

        // Waiting
        int Waited = 3;
        while (Client is null)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("                 ");
            Console.SetCursorPosition(0, 0);

            Console.Write("Connecting");
            for (int i = 0; i < Waited; i++)
            {
                Console.Write(".");
            }
            Console.WriteLine();

            Waited += 1;
            if (Waited == 5)
                Waited = 0;

            Console.WriteLine("IP and Port to use: " + LocalIP + ":" + PortUsed);

            Thread.Sleep(200);
        }

        NetworkStream = Client.GetStream();

        Data = new ConcurrentQueue<byte>();

        Task DataStreamReader = new Task(() =>
        {
            // Inecfective but it is what it is :D
            while (Client is not null && NetworkStream is not null && Client.Connected)
            {
                while (true)
                {
                    int data = NetworkStream.ReadByte();
                    if (data == -1)
                        break;
                    Data.Enqueue((byte)data);
                }

                Thread.Sleep(20); // 20ms = ~50hrz

                if (!GameRunning)
                    return;
            }
        }
        );

        DataStreamReader.Start();

        Console.Clear();
    }

    // Functions for the GameLoop

    /// <summary>
    /// Gets the last send byte and treats that as the move
    /// </summary>
    /// <returns></returns>
    private int TryGetMove()
    {
        int move = -1;
        while (Data.Count != 0 && Data.TryDequeue(out byte Value))
        {
            move = Value;
        }

        if (move < 0 || move > 8)
            return -1;
        else
            return move;
    }

    /// <summary>
    /// Sends a value between 0 and 8 to the other player
    /// </summary>
    private void SendMove(int move)
    {
        if (move < 0 || move > 8)
            return;

        if (NetworkStream is not null)
            NetworkStream.WriteByte((byte)move);
    }

    ~Game()
    {
        if (NetworkStream is not null)
            NetworkStream.Dispose();
        if (Client is not null)
            Client.Dispose();
        if (OpponentDataStream is not null)
            OpponentDataStream.Dispose();
        if (tcpListener is not null)
            tcpListener.Stop();
        GameRunning = false;
    }
}
