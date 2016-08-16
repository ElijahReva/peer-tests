namespace Peer
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    using Lidgren.Network;

    using SshTest.Peer;

    public class Program
    {
        public static void Main(string[] args)
        {
            var scanner = new Scanner();
            bool connected = false;
            NetPeer server = null;
            while (!connected)
            {
                var config = new NetPeerConfiguration("loosers");
                var nextFreePort = BindingPool.GetNextFreePort();
                if (nextFreePort == -1)
                {
                    return;
                }
                config.Port = nextFreePort;
                Console.WriteLine("Peer port = {0}", config.Port);
                config.AcceptIncomingConnections = true;
                config.PingInterval = 0.5f;
                server = new NetPeer(config);
                try
                {
                    server.Start();
                    connected = true;
                }
                catch (Exception e)
                {
                    BindingPool.SetClosed(config.Port);

                    Console.WriteLine("Peer port i closed - {0}", config.Port);
                }
            }
            Console.WriteLine("Server started");
            Console.WriteLine("Reader - 1, Writer - 2");
            var flag = scanner.NextInt();
            if (flag == 1)
            {                       
                StartServer(server);
            }
            if (flag == 2)
            {
                StartClient(scanner, server);
            }

        }

        private static void StartClient(Scanner scanner, NetPeer server)
        {
            Console.WriteLine("Write friend port");
            var clientPort = scanner.NextInt();
            var connection = server.Connect(new IPEndPoint(IPAddress.Loopback, clientPort));

            while (true)
            {
                NetOutgoingMessage sendMsg = server.CreateMessage();
                Console.WriteLine("Write message");
                sendMsg.Write(scanner.Next());
                server.SendMessage(sendMsg, connection, NetDeliveryMethod.ReliableOrdered);
            }
        }

        private static void StartServer(NetPeer server)
        {
            NetIncomingMessage msg;
            while (true)
            {
                while ((msg = server.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.StatusChanged:
                            Console.WriteLine(msg.SenderConnection);
                            break;
                        case NetIncomingMessageType.Data:
                            Console.WriteLine(msg.ReadString());
                            break;
                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:
                            Console.WriteLine(msg.ReadString());
                            break;
                        default:
                            Console.WriteLine("Unhandled type: " + msg.MessageType);
                            break;
                    }
                    server.Recycle(msg);
                }
            }
        }
    }

    public class EncryptedServer
    {
        private TcpListener listner;

        private int requestCount;

        private TcpClient clientSocket;

        public EncryptedServer()
        {
            listner = new TcpListener(IPAddress.Loopback, 6666);
            requestCount = 0;
            clientSocket = default(TcpClient);
        }

        public void Start()
        {


            listner.Start();
            Console.WriteLine(" >> Server Started");
            clientSocket = listner.AcceptTcpClient();
            Console.WriteLine(" >> Accept connection from client");
            requestCount = 0;

            while ((true))
            {
                try
                {
                    requestCount = requestCount + 1;
                    var networkStream = clientSocket.GetStream();
                    byte[] bytesFrom = new byte[1024];
                    networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                    string dataFromClient = GetString(bytesFrom);
                    Console.WriteLine(" >> Data from client - " + dataFromClient);
                    var serverResponse = "Last Message from client" + dataFromClient;
                    networkStream.Flush();
                    Console.WriteLine(" >> " + serverResponse);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            clientSocket.Close();
            listner.Stop();
            Console.WriteLine(" >> exit");
            Console.ReadLine();
        }
        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

    }
}
