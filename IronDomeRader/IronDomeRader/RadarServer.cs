using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IronDomeRader
{
    internal class RadarServer
    {
        TcpListener listener;
        const int port=6767;
        const string address = "127.0.0.1";

        public RadarServer()
        {
            listener = new TcpListener(IPAddress.Parse(address),port);
        }
        public async Task RaderServerListenAsync()
        {
            listener.Start();

            TcpClient commandClient = await listener.AcceptTcpClientAsync();

            NetworkStream stream = commandClient.GetStream();

            string message = "Hello Command";
            byte[] data = Encoding.UTF8.GetBytes(message);

            await stream.WriteAsync(data, 0, data.Length);
        }
    }
}
