using FluentValidation;

namespace grpc_server.Validator;

public class UserRequestValidator : AbstractValidator<UserRequest>
{
    public UserRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId is required.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");
    }
}