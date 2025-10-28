using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;

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
                var errors = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
                throw new RpcException(new Status(StatusCode.InvalidArgument, errors));
            }
        }

        // Continue with actual gRPC method
        return await continuation(request, context);
    }
}
