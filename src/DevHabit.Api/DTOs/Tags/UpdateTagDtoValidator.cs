using FluentValidation;

namespace DevHabit.Api.DTOs.Tags;

public sealed class UpdateTagDtoValidator : AbstractValidator<UpdateTagDto>
{
    public UpdateTagDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(100);
    }
}
