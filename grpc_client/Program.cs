using Grpc.Core;
using Grpc.Net.Client;
using grpc_client;
using Microsoft.VisualBasic;
using System.Threading.Tasks;

internal partial class Program
{
    private static async Task Main(string[] args)
    {
        await CallGrpc();
        //Task.Run(async () => await CallGrpc()).Wait();
    }

    static async Task CallGrpc()
    {
        //Adjust your server address if needed
        using var channel = GrpcChannel.ForAddress("https://localhost:7241");
        var client = new Demo.DemoClient(channel);

        Console.WriteLine("=== Unary Call ===");
        await UnaryCallExample(client);

        Console.WriteLine("\n=== Server Streaming ===");
        await ServerStreamingExample(client);

        Console.WriteLine("\n=== Client Streaming ===");
        await ClientStreamingExample(client);

        Console.WriteLine("\n=== Bi-directional Streaming ===");
        await BidirectionalStreamingExample(client);
    }

    // 1️⃣ Unary Example
    static async Task UnaryCallExample(Demo.DemoClient client)
    {
        var request = new UserRequest
        {
            UserId = 0,
            Username = "Alice",
            Score = 95.5,
            IsOnline = true,
            Roles = { "Admin", "Editor" },
            Attributes = { { "Region", "US" }, { "Device", "Desktop" } },
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        var response = await client.GetUserInfoAsync(request);
        Console.WriteLine($"Response: {response.Message}, Score: {response.ProcessedScore}");
    }

    // 2️⃣ Server Streaming Example
    static async Task ServerStreamingExample(Demo.DemoClient client)
    {
        var request = new UserRequest { UserId = 2, Username = "Bob", Score = 50.0 };
        using var call = client.StreamUserNotifications(request);

        await foreach (var response in call.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine($"[ServerStream] {response.Message}, ProcessedScore: {response.ProcessedScore}");
        }
    }

    // 3️⃣ Client Streaming Example
    static async Task ClientStreamingExample(Demo.DemoClient client)
    {
        using var call = client.UploadUserActivity();

        for (int i = 0; i < 3; i++)
        {
            var request = new UserRequest
            {
                UserId = 10 + i,
                Username = "Charlie",
                Score = 10 + i * 5,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            await call.RequestStream.WriteAsync(request);
            Console.WriteLine($"Sent activity {i + 1}");
            await Task.Delay(300);
        }

        await call.RequestStream.CompleteAsync();

        var response = await call;
        Console.WriteLine($"[ClientStream] {response.Message}");
    }

    // 4️⃣ Bi-Directional Streaming Example
    static async Task BidirectionalStreamingExample(Demo.DemoClient client)
    {
        using var call = client.ChatWithUser();

        // Send messages asynchronously
        var sendTask = Task.Run(async () =>
        {
            for (int i = 1; i <= 3; i++)
            {
                var request = new UserRequest
                {
                    UserId = i,
                    Username = $"User{i}",
                    Score = i * 10,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
                await call.RequestStream.WriteAsync(request);
                Console.WriteLine($"Sent message {i}");
                await Task.Delay(400);
            }
            await call.RequestStream.CompleteAsync();
        });

        // Read responses asynchronously
        var readTask = Task.Run(async () =>
        {
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine($"[ChatResponse] {response.Message}, Score: {response.ProcessedScore}");
            }
        });

        await Task.WhenAll(sendTask, readTask);
    }
}