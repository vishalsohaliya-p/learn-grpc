namespace grpc_client.Model;

public class ValidationErrorResponse
{
    public List<ValidationError> Errors { get; set; } = new();
}
