using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IronDomeCommandCenter
{
    struct InterceptorData
    {
        public int CommandId { get; set; }
        public int TargetId { get; set; }
        public int InterceptorId { get; set; }

        public double InterceptorX { get; set; }
        public double InterceptorY { get; set; }
        public double InterceptorVx { get; set; }
        public double InterceptorVy { get; set; }

        public InterceptorData(
            int commandId,
            int targetId,
            int interceptorId,
            double x,
            double y,
            double vx,
            double vy)
        {
            CommandId = commandId;
            TargetId = targetId;
            InterceptorId = interceptorId;

            InterceptorX = x;
            InterceptorY = y;
            InterceptorVx = vx;
            InterceptorVy = vy;
        }
    }
    internal class InterceptorServer
    {
        private TcpListener listener;

        private const int port = 6768;
        private const string address = "127.0.0.1";

        public NetworkStream stream;

        public InterceptorServer()
        {
            listener = new TcpListener(
              IPAddress.Parse(address),
              port
          );
        }
        public async Task CommandServerListenAsync()
        {
            listener.Start();

            TcpClient commandClient =
                await listener.AcceptTcpClientAsync();

            stream = commandClient.GetStream();
        }

        public async Task SendInterceptCommandAsync(InterceptCommand command)
        {
            string json = JsonSerializer.Serialize(command);

            byte[] data =
                Encoding.UTF8.GetBytes(json + "\n");

            await stream.WriteAsync(data, 0, data.Length);
        }
        
    }
}
