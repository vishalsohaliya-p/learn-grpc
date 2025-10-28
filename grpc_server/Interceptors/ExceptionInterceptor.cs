using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Text.Json;

namespace grpc_server.Interceptors;

public class ExceptionInterceptor : Interceptor
{
    private readonly ILogger<ExceptionInterceptor> _logger;

    public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            // Let the normal gRPC call continue
            return await continuation(request, context);
        }
        catch (RpcException) // Already handled (like validation errors)
        {
            throw;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Not found: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.PermissionDenied, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            var errorResponse = new
            {
                error = new
                {
                    message = "An unexpected error occurred.",
                    detail = ex.Message
                }
            };

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { WriteIndented = true });

            throw new RpcException(new Status(StatusCode.Internal, json));
        }
    }
}
