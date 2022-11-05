using TicTacToeEngine;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
using System.Text;

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


    enum IDType
    {
        Move = 1,
        Message = 2,
    }
    List<(byte ID, byte[] Data)> Batches = new List<(byte ID, byte[] Data)>();

    TicTacToeGameLoop gameLoop;

    public Game()
    {
        gameLoop = new TicTacToeGameLoop(SendMove, TryGetMove, SendMessage, TryGetMessage);
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

    private void UpdateBatches()
    {
        while (NetworkStream is not null)
        {
            // Get id of data or break if none
            if (!Data.TryDequeue(out byte ID))
                break;

            if (ID == (int)IDType.Move)
            {
                byte[] arr = new byte[1];
                while (true)
                {
                    if (Data.TryDequeue(out byte Value))
                    {
                        arr[0] = Value;
                        break;
                    }
                }
                Batches.Add(((byte)ID, arr));
            }

            // A Messege
            else if (ID == 2)
            {
                List<byte> bytes = new List<byte>();

                bytes.Add(2);

                for (int i = 0; i < 4; i++)
                {
                    if (Data.TryDequeue(out byte value))
                        bytes.Add(value);
                    else
                        i--;
                }

                int size = BitConverter.ToInt32(bytes.Take(new Range(1, 5)).ToArray(), 0);

                for (int i = 0; i < size; i++)
                {
                    if (Data.TryDequeue(out byte value))
                        bytes.Add(value);
                    else
                        i--;
                }


                Batches.Add((ID, bytes.Take(new Range(5, bytes.Count)).ToArray()));
            }
        }
    }

    // Functions for the GameLoop

    /// <summary>
    /// Get the next message in Batches
    /// </summary>
    /// <returns></returns>
    private (bool MessageReady, string Message) TryGetMessage()
    {
        UpdateBatches();

        string message = "";
        for (int i = 0; i < Batches.Count; i++)
        {
            if (Batches[i].ID == (int)IDType.Message)
            {
                message = Encoding.UTF8.GetString(Batches[i].Data);

                Batches.RemoveAt(i);

                return (true, message);
            }
        }

        return (false, "NULL");
    }

    private void SendMessage(string message)
    {
        List<byte> bytesList = new List<byte>(message.Length + 5);
        bytesList.Add(2);
        bytesList.AddRange(BitConverter.GetBytes(message.Length));
        bytesList.AddRange(Encoding.UTF8.GetBytes(message, 0, message.Length));

        if (NetworkStream is not null)
            NetworkStream.Write(bytesList.ToArray(), 0, bytesList.Count);
    }

    /// <summary>
    /// Gets the last send byte and treats that as the move
    /// </summary>
    /// <returns></returns>
    private int TryGetMove()
    {
        UpdateBatches();

        int move = -1;
        for (int i = 0; i < Batches.Count; i++)
        {
            if (Batches[i].ID == 1)
            {
                move = (int)Batches[i].Data[0];
                Batches.RemoveAt(i);
                i--;
            }
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
        {
            byte[] arr = new byte[2];
            arr[0] = 1;
            arr[1] = (byte)move;
            NetworkStream.Write(arr, 0, 2);
        }
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
