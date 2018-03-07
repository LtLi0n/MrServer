using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace MrServer.Network
{
    public static class NetworkExtensions
    {
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }
    }
}
