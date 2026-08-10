using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
namespace IronDomeCommandCenter
{
    struct TargetData
    {
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Vx { get; set; }
        public double Vy { get; set; }

        public TargetData(string name, double x, double y, double vx, double vy)
        {
            Name = name;
            X = x;
            Y = y;
            Vx = vx;
            Vy = vy;
        }
    }
    class CommandClient
    {
        const int port = 6767;
        const string address="127.0.0.1";
        
        StreamReader reader;
        TcpClient client;
        public CommandClient()
        {
            client = new TcpClient();
        }
        public async Task ConnectToRadarAsync()
        {
            await client.ConnectAsync(address, port);

            NetworkStream stream = client.GetStream();
            reader = new StreamReader(stream,Encoding.UTF8);
        }
        public async Task<TargetData?> CommandClientReadAsync()
        {
            string? json = await reader.ReadLineAsync();
            if (json == null)
            {
                return null;
            }

            TargetData target = JsonSerializer.Deserialize<TargetData>(json);

            return target;
        }
    }
}
