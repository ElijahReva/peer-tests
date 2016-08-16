namespace Peer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;

    using Lidgren.Network;

    public static class BindingPool
    {
        private static TcpConnectionInformation[] tcpConnInfoArray;

        private static IPGlobalProperties ipGlobalProperties;

        private static readonly IDictionary<int, bool> allPorts = new Dictionary<int, bool>(portCount);

        private const int portCount = 10;

        static BindingPool()
        {
            for (int i = 6666; i < 6676; i++)
            {
                allPorts.Add(i, true);
            }
        }

        public static int GetNextFreePort()
        {
            foreach (var port in allPorts.Where(port => port.Value))
            {
                return port.Key;
            }
            return -1;
        }

        public static void SetClosed(int port)
        {
            allPorts.Remove(port);
            allPorts.Add(port, false);
        }

        private static bool CheckAvailableServerPort(int port)
        {
            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.

            return IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().All(endpoint => endpoint.LocalEndPoint.Port != port);
        }
        private static bool CheckPort(int port)
        {
            var config = new NetPeerConfiguration("loosers") { Port = port };

            var server = new NetPeer(config);
            try
            {
                server.Start();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                server.Shutdown("bye");
            }
            return true;
        }

        private static void UpdateConnections()
        {
            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

        }

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}