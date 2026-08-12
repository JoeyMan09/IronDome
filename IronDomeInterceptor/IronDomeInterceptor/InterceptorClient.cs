using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IronDomeInterceptor
{
    struct TargetData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Vx { get; set; }
        public double Vy { get; set; }

        public TargetData(int id, string name, double x, double y, double vx, double vy)
        {
            Id = id;
            Name = name;
            X = x;
            Y = y;
            Vx = vx;
            Vy = vy;
        }
        public double GetSpeed()
        {
            if (X < 0 || Y < 0)
            {
                return -Math.Sqrt(X * X + Y * Y);
            }
            else
                return Math.Sqrt(X * X + Y * Y);
        }
        public override string ToString()
        {
            return $"ID: {Id} | {Name} | " +
            $"X: {X:F0} | Y: {Y:F0} | " +
            $"Speed: {GetSpeed():F0}";
        }
    }
    
       
    
    internal class InterceptorClient
    {
        const int port = 6768;
        const string address = "127.0.0.1";

        StreamReader reader;
        TcpClient client;
        public InterceptorClient()
        {
            client = new TcpClient();
        }
        public async Task ConnectToCommandAsync()
        {
            await client.ConnectAsync(address, port);

            NetworkStream stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
        }
        public async Task<InterceptCommand?> InterceptorClientReadAsync()
        {
            string? json = await reader.ReadLineAsync();

            if (json == null)
                return null;

            InterceptCommand? command =
                JsonSerializer.Deserialize<InterceptCommand>(json);

            return command;
        }
    }
}
