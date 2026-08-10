using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IronDomeRader
{
    public struct TargetData
    {
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Vx { get; set; }
        public double Vy { get; set; }

        public TargetData(
            string name,
            double x,
            double y,
            double vx,
            double vy)
        {
            Name = name;
            X = x;
            Y = y;
            Vx = vx;
            Vy = vy;
        }
    }

    internal class RadarServer
    {
        private TcpListener listener;

        private const int port = 6767;
        private const string address = "127.0.0.1";

        public NetworkStream stream;

        public RadarServer()
        {
            listener = new TcpListener(
                IPAddress.Parse(address),
                port
            );
        }

        public async Task RaderServerListenAsync()
        {
            listener.Start();

            TcpClient commandClient =
                await listener.AcceptTcpClientAsync();

            stream = commandClient.GetStream();
        }

        public async Task SendTargetAsync(TargetData target)
        {
            string json =
                JsonSerializer.Serialize(target);

            byte[] data =
                Encoding.UTF8.GetBytes(json + "\n");

            await stream.WriteAsync(
                data,
                0,
                data.Length
            );
        }
    }
}