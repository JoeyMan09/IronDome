using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IronDomeCommandCenter
{//
    class CommandClient
    {
        const int port = 6767;
        TcpClient client;
        const string address="127.0.0.1";

        public CommandClient()
        {
            client = new TcpClient();
        }
        public async Task ConnectToRadarAsync()
        {
            await client.ConnectAsync(address, port);
        }
        public async Task<string> CommandClientReadAsync()
        {
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[1024];

            int bytesRead =
                await stream.ReadAsync(buffer, 0, buffer.Length);

            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
    }
}
