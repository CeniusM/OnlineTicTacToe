using TicTacToeEngine;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;

namespace TicTacToeGame;

internal class Game
{
    Func<bool> HasWifi = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable;

    const int PortUsed = 4123;
    string LocalIP = "\0";

    // Used by other side
    //Client = new TcpClient(new IPEndPoint(IPAddress.Parse(LocalIP), PortUsed));
    TcpClient? Client = null;
    NetworkStream? NetworkStream = null;
    TcpListener? Listener = null;

    NetworkStream? OpponentDataStream = null;


    TicTacToe game = new TicTacToe();

    public Game()
    {

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

        EstablishConnectionWithOpponent();
    }

    private async void EstablishConnectionWithOpponent()
    {
        if (Client is not null)
        {
            Client.Dispose();
            Client = null;
        }


        TcpListener tcpListener = new TcpListener(IPAddress.Parse(LocalIP), PortUsed);

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
        Console.WriteLine("Connection Made :D");
        Thread.Sleep(1000);
        Console.Clear();
        GameLoop();

        Console.ReadLine();
    }

    /// <summary>
    /// Used for when there is a connection
    /// </summary>
    private void GameLoop()
    {
        ConcurrentQueue<byte> Data = new ConcurrentQueue<byte>();

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
            }
        }
        );
        DataStreamReader.Start();

        Console.WriteLine("Data: ");

        try
        {
            while (true)
            {
                while (Data.Count != 0)
                {
                    if (Data.TryDequeue(out byte Value))
                    {
                        Console.Write((char)Value);
                        NetworkStream.WriteByte(Value);
                    }
                }

                Thread.Sleep(20);
            }
        }
        catch (Exception e)
        {

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
    }
}
