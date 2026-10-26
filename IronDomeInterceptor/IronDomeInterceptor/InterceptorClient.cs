using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IronDomeInterceptor
{
    internal class InterceptorClient
    {
        private const int port = 6768;
        private const string address = "127.0.0.1";

        private readonly TcpClient client;

        private StreamReader? reader;
        private StreamWriter? writer;

        public InterceptorClient()
        {
            client = new TcpClient();
        }

        public async Task ConnectToCommandAsync()
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

        public async Task<InterceptCommand?>
            InterceptorClientReadAsync()
        {
            if (reader == null)
                throw new InvalidOperationException(
                    "Not connected to Command Center."
                );

            string? json = await reader.ReadLineAsync();

            if (json == null)
                return null;

            return JsonSerializer.Deserialize<InterceptCommand>(json);
        }

        public async Task SendInterceptResultAsync(
            int commandId,
            int targetId,
            bool success)
        {
            if (writer == null)
                throw new InvalidOperationException(
                    "Not connected to Command Center."
                );

            InterceptResult result = new InterceptResult
            {
                CommandId = commandId,
                TargetId = targetId,
                Success = success
            };

            string json = JsonSerializer.Serialize(result);

            await writer.WriteLineAsync(json);
        }
    }
}