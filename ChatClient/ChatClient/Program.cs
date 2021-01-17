using System;
using System.Threading;
using System.Threading.Tasks;
using ChatClient.Protos;
using Grpc.Net.Client;

namespace ChatClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Write("Please enter your name: ");
            var username = Console.ReadLine();

            var channel = GrpcChannel.ForAddress("http://localhost:5001");
            var client = new ChatService.ChatServiceClient(channel);

            using (var chat = client.Join())
            {
                _ = Task.Run(async () =>
                {
                    while (await chat.ResponseStream.MoveNext(cancellationToken: CancellationToken.None))
                    {
                        var response = chat.ResponseStream.Current;
                        Console.WriteLine($"{response.User}: {response.Text}");
                    }
                });

                await chat.RequestStream.WriteAsync(new Message { User = username, Text = $"{username} has joined the room" });

                string line;
                while ((line = Console.ReadLine()) != null)
                {
                    if (line.ToLower() == "bye")
                    {
                        break;
                    }
                    await chat.RequestStream.WriteAsync(new Message { User = username, Text = line });
                }
                await chat.RequestStream.CompleteAsync();
            }

            Console.WriteLine("Disconnecting");
            await channel.ShutdownAsync();
        }
    }
}
