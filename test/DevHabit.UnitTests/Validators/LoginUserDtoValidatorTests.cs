using DevHabit.Api.DTOs.Auth;
using FluentValidation.TestHelper;

namespace DevHabit.UnitTests.Validators;

public sealed class LoginUserDtoValidatorTests
{
    private readonly LoginUserDtoValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldNotReturnError_WhenAllPropertiesAreValid()
    {
        // Arrange
        var dto = new LoginUserDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        // Act
        TestValidationResult<LoginUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenEmailIsEmpty()
    {
        // Arrange
        var dto = new LoginUserDto
        {
            Email = string.Empty,
            Password = "password123"
        };

        // Act
        TestValidationResult<LoginUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenEmailIsInvalid()
    {
        // Arrange
        var dto = new LoginUserDto
        {
            Email = "invalid-email",
            Password = "password123"
        };

        // Act
        TestValidationResult<LoginUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenEmailExceedsMaxLength()
    {
        // Arrange
        var dto = new LoginUserDto
        {
            Email = $"{new string('a', 95)}@test.com",
            Password = "password123"
        };

        // Act
        TestValidationResult<LoginUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenPasswordIsEmpty()
    {
        // Arrange
        var dto = new LoginUserDto
        {
            Email = "test@example.com",
            Password = string.Empty
        };

        // Act
        TestValidationResult<LoginUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenPasswordIsTooShort()
    {
        // Arrange
        var dto = new LoginUserDto
        {
            Email = "test@example.com",
            Password = "12345"
        };

        // Act
        TestValidationResult<LoginUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenPasswordExceedsMaxLength()
    {
        // Arrange
        var dto = new LoginUserDto
        {
            Email = "test@example.com",
            Password = new string('a', 101)
        };

        // Act
        TestValidationResult<LoginUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
