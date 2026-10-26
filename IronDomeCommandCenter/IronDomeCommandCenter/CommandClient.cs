using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using static IronDomeCommandCenter.ImpactLocation;
namespace IronDomeCommandCenter
{
    struct TargetData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Vx { get; set; }
        public double Vy { get; set; }
        public Entitytype.EntityType EntityType { get; set; }
        public bool isFriendly { get; set; }
        public double impactX { get; set; }
        public double impactY { get; set; }
        public ImpactLocation.ImpactLocations impactLocations { get; set; }

        public TargetData(int id, string name, double x, double y,
            double vx, double vy, Entitytype.EntityType entityType,
            bool friendly, double impactX, double impactY,
            ImpactLocation.ImpactLocations impactLocation)
        {
            Id = id;
            Name = name;
            X = x;
            Y = y;
            Vx = vx;
            Vy = vy;
            EntityType = entityType;
            isFriendly = friendly;
            this.impactX = impactX;
            this.impactY = impactY;
            impactLocations = impactLocation;
        }

        public double GetSpeed() => Math.Sqrt(Vx * Vx + Vy * Vy);

        public double GetTimeToImpact()
        {
            double dx = impactX - X;
            double dy = impactY - Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance < 0.001) return 0;
            double speed = GetSpeed();
            if (speed < 0.001 || dx * Vx + dy * Vy <= 0)
                return double.PositiveInfinity;
            return distance / speed;
        }

        public string EtaDisplay
        {
            get
            {
                double eta = GetTimeToImpact();
                return double.IsInfinity(eta) || double.IsNaN(eta)
                    ? "N/A" : $"{eta:F1}s";
            }
        }

        public override string ToString() =>
            $"ID: {Id} | {Name} | X: {X:F0} | Y: {Y:F0} | Speed: {GetSpeed():F0} | ETA: {EtaDisplay}";
    }
    internal class CommandClient
    {
        private const int port = 6767;
        private const string address = "127.0.0.1";

        private readonly TcpClient client;

        private StreamReader? reader;
        private StreamWriter? writer;

        private readonly SemaphoreSlim sendLock = new(1, 1);

        public CommandClient()
        {
            client = new TcpClient();
        }

        public async Task ConnectToRadarAsync()
        {
            await client.ConnectAsync(address, port);

            NetworkStream stream = client.GetStream();

            reader = new StreamReader(
                stream,
                Encoding.UTF8,
                leaveOpen: true
            );

            writer = new StreamWriter(
                stream,
                new UTF8Encoding(false),
                leaveOpen: true
            )
            {
                AutoFlush = true
            };
        }

        public async Task<TargetData?> CommandClientReadAsync()
        {
            if (reader == null)
                throw new InvalidOperationException(
                    "Radar is not connected."
                );

            string? json = await reader.ReadLineAsync();

            if (json == null)
                return null;

            return JsonSerializer.Deserialize<TargetData>(json);
        }

        public async Task SendTargetDestroyedAsync(int targetId)
        {
            if (writer == null)
                throw new InvalidOperationException(
                    "Radar is not connected."
                );

            RadarMessage message = new RadarMessage
            {
                MessageType = "TargetDestroyed",
                TargetId = targetId
            };

            string json = JsonSerializer.Serialize(message);

            await sendLock.WaitAsync();

            try
            {
                await writer.WriteLineAsync(json);
            }
            finally
            {
                sendLock.Release();
            }
        }
    }

    internal class RadarMessage
    {
        public string MessageType { get; set; } = "";
        public int TargetId { get; set; }
    } 
}
