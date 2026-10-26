using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IronDomeCommandCenter
{
    internal struct InterceptorData
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
        private readonly TcpListener listener;

        private NetworkStream? stream;
        private StreamReader? reader;

        private readonly SemaphoreSlim sendLock = new(1, 1);

        public event Func<InterceptResult, Task>? InterceptCompleted;

        public InterceptorServer()
        {
            listener = new TcpListener(
                IPAddress.Loopback,
                6768
            );
        }

        public async Task CommandServerListenAsync()
        {
            listener.Start();

            TcpClient interceptorClient =
                await listener.AcceptTcpClientAsync();

            stream = interceptorClient.GetStream();

            reader = new StreamReader(
                stream,
                Encoding.UTF8,
                leaveOpen: true
            );

            _ = ListenForInterceptResultsAsync();
        }

        public async Task SendInterceptCommandAsync(
            InterceptCommand command)
        {
            if (stream == null)
                throw new InvalidOperationException(
                    "Interceptor is not connected."
                );

            string json = JsonSerializer.Serialize(command);

            byte[] data = Encoding.UTF8.GetBytes(
                json + "\n"
            );

            await sendLock.WaitAsync();

            try
            {
                await stream.WriteAsync(
                    data,
                    0,
                    data.Length
                );
            }
            finally
            {
                sendLock.Release();
            }
        }

        private async Task ListenForInterceptResultsAsync()
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

                    InterceptResult? result =
                        JsonSerializer.Deserialize<InterceptResult>(json);

                    if (result == null)
                        continue;

                    if (InterceptCompleted == null)
                        continue;

                    foreach (
                        Func<InterceptResult, Task> handler
                        in InterceptCompleted.GetInvocationList())
                    {
                        await handler(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Interceptor receive error: {ex.Message}"
                );
            }
        }
    }
}