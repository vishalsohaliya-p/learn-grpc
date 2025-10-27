using Grpc.Core;

namespace grpc_server.Services;

public class DemoService: Demo.DemoBase
{
    public override Task<UserResponse> GetUserInfo(UserRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UserResponse
        {
            ResponseId = request.UserId,
            Message = $"User {request.Username} retrieved successfully.",
            ProcessedScore = request.Score * 1.2,
            Notifications = { "User info fetched", "Profile loaded" },
            Metadata = { { "is_online", request.IsOnline.ToString() } },
            ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });
    }

    public override async Task StreamUserNotifications(UserRequest request, IServerStreamWriter<UserResponse> responseStream, ServerCallContext context)
    {
        for (int i = 1; i <= 5; i++)
        {
            await responseStream.WriteAsync(new UserResponse
            {
                ResponseId = i,
                Message = $"Notification {i} for {request.Username}",
                ProcessedScore = request.Score + i,
                ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
            await Task.Delay(500);
        }
    }

    public override async Task<UserResponse> UploadUserActivity(IAsyncStreamReader<UserRequest> requestStream, ServerCallContext context)
    {
        double totalScore = 0;
        int activityCount = 0;

        await foreach (var request in requestStream.ReadAllAsync())
        {
            totalScore += request.Score;
            activityCount++;
        }

        return new UserResponse
        {
            Message = $"Received {activityCount} user activities, total score: {totalScore}",
            ProcessedScore = totalScore,
            ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public override async Task ChatWithUser(IAsyncStreamReader<UserRequest> requestStream, IServerStreamWriter<UserResponse> responseStream, ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(new UserResponse
            {
                ResponseId = request.UserId,
                Message = $"Chat echo from server: {request.Username}",
                ProcessedScore = request.Score * 2,
                ServerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
        }
    }
}
