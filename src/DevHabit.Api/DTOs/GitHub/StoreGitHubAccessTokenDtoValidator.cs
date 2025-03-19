using FluentValidation;

namespace DevHabit.Api.DTOs.GitHub;

public sealed class StoreGitHubAccessTokenDtoValidator : AbstractValidator<StoreGitHubAccessTokenDto>
{
    public StoreGitHubAccessTokenDtoValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.ExpiresInDays)
            .GreaterThan(0)
            .LessThanOrEqualTo(365); // Maximum 1 year expiration
    }
}
