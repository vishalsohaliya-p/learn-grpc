namespace grpc_server.Model;

public class ValidationErrorResponse
{
    public List<ValidationError> Errors { get; set; } = new();
}
