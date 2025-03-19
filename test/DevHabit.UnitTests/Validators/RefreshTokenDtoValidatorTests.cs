using DevHabit.Api.DTOs.Auth;
using FluentValidation.TestHelper;

namespace DevHabit.UnitTests.Validators;

public sealed class RefreshTokenDtoValidatorTests
{
    private readonly RefreshTokenDtoValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldNotReturnError_WhenRefreshTokenIsValid()
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = "valid-refresh-token"
        };

        // Act
        TestValidationResult<RefreshTokenDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_ShouldReturnError_WhenRefreshTokenIsInvalid(string? refreshToken)
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = refreshToken!
        };

        // Act
        TestValidationResult<RefreshTokenDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
} 