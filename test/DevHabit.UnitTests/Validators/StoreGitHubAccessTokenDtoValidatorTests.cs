using DevHabit.Api.DTOs.GitHub;
using FluentValidation.TestHelper;

namespace DevHabit.UnitTests.Validators;

public sealed class StoreGitHubAccessTokenDtoValidatorTests
{
    private readonly StoreGitHubAccessTokenDtoValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldNotReturnError_WhenAccessTokenIsValid()
    {
        // Arrange
        var dto = new StoreGitHubAccessTokenDto
        {
            AccessToken = "gho_valid_access_token",
            ExpiresInDays = 30
        };

        // Act
        TestValidationResult<StoreGitHubAccessTokenDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_ShouldReturnError_WhenAccessTokenIsInvalid(string? accessToken)
    {
        // Arrange
        var dto = new StoreGitHubAccessTokenDto
        {
            AccessToken = accessToken!,
            ExpiresInDays = 30
        };

        // Act
        TestValidationResult<StoreGitHubAccessTokenDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccessToken);
    }
} 
