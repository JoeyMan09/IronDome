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

        private StreamReader reader;

        private TcpClient client;


        public InterceptorClient()
        {
            client = new TcpClient();
        }


        public async Task ConnectToCommandAsync()
        {
            await client.ConnectAsync(
                address,
                port
            );

            NetworkStream stream =
                client.GetStream();

            reader =
                new StreamReader(
                    stream,
                    Encoding.UTF8
                );
        }


        public async Task<InterceptCommand?>
            InterceptorClientReadAsync()
        {
            string? json =
                await reader.ReadLineAsync();

            if (json == null)
                return null;

            InterceptCommand? command =
                JsonSerializer.Deserialize<InterceptCommand>(
                    json
                );

            return command;
        }
    }
}