using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using grpc_server.Model;
using System.Text.Json;

namespace grpc_server.Interceptors;

public class ValidationInterceptor : Interceptor
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        // Find the validator for the current request type
        var validatorType = typeof(IValidator<>).MakeGenericType(request.GetType());
        var validator = _serviceProvider.GetService(validatorType) as IValidator;

        if (validator != null)
        {
            var result = await validator.ValidateAsync(new ValidationContext<object>(request));

            if (!result.IsValid)
            {
                // Create structured response
                var response = new ValidationErrorResponse
                {
                    Errors = result.Errors
                        .Select(e => new ValidationError
                        {
                            Field = e.PropertyName,
                            Message = e.ErrorMessage
                        })
                        .ToList()
                };

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // Return structured error
                throw new RpcException(new Status(StatusCode.InvalidArgument, json));
            }
        }


        // Continue with actual gRPC method
        return await continuation(request, context);
    }
}
