using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static IronDomeRader.ImpactLocation;

namespace IronDomeRader
{
    public struct TargetData
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public double X { get; set; }
        public double Y { get; set; }

        public double Vx { get; set; }
        public double Vy { get; set; }

        public ThreatLevel.threatLevel threatLevel { get; set; }

        public EntityType.Entitytype EntityType { get; set; }
        public bool isFriendly {  get; set; }
        public double impactX { get; set; }
        public double impactY { get; set; }
        public ImpactLocation.ImpactLocations impactLocations { get; set; }
        public TargetData(
            int id,
            string name,
            double x,
            double y,
            double vx,
            double vy,
            ThreatLevel.threatLevel threatLevel,
            EntityType.Entitytype entityType,bool friend,double impactx, double impacty,ImpactLocation.ImpactLocations impactLocation)
        {
            Id = id;

            Name = name;

            X = x;
            Y = y;

            Vx = vx;
            Vy = vy;

            this.threatLevel = threatLevel;

            EntityType = entityType;
            isFriendly= friend;
            impactX = impactx;
            impactY = impacty;
            impactLocations = impactLocation;
        }
    }


    internal class RadarServer
    {
        private readonly TcpListener listener;
        private NetworkStream? stream;
        private StreamReader? reader;

        private readonly SemaphoreSlim sendLock = new(1, 1);

        public event Action<int>? TargetDestroyed;

        public RadarServer()
        {
            listener = new TcpListener(
                IPAddress.Loopback, 6767
            );
        }

        public async Task RaderServerListenAsync()
        {
            listener.Start();

            TcpClient client = await listener.AcceptTcpClientAsync();
            stream = client.GetStream();

            reader = new StreamReader(
                stream, Encoding.UTF8, leaveOpen: true
            );

            _ = ListenForDestroyedTargetsAsync();
        }

        public async Task SendTargetAsync(TargetData target)
        {
            if (stream == null)
                throw new InvalidOperationException(
                    "Command Center not connected."
                );

            byte[] data = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(target) + "\n"
            );

            await sendLock.WaitAsync();

            try
            {
                await stream.WriteAsync(data, 0, data.Length);
            }
            finally
            {
                sendLock.Release();
            }
        }

        private async Task ListenForDestroyedTargetsAsync()
        {
            if (reader == null)
                return;

            try
            {
                while (true)
                {
                    string? json = await reader.ReadLineAsync();

                    if (json == null)
                        break;

                    RadarMessage? message =
                        JsonSerializer.Deserialize<RadarMessage>(json);

                    if (message?.MessageType == "TargetDestroyed")
                    {
                        TargetDestroyed?.Invoke(message.TargetId);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Radar error: {ex.Message}"
                );
            }
        }
    }

    internal class RadarMessage
    {
        public string MessageType { get; set; } = "";
        public int TargetId { get; set; }
    }
}