using DevHabit.Api.DTOs.Auth;
using FluentValidation.TestHelper;

namespace DevHabit.UnitTests.Validators;

public sealed class RegisterUserDtoValidatorTests
{
    private readonly RegisterUserDtoValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldNotReturnError_WhenAllPropertiesAreValid()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        // Act
        TestValidationResult<RegisterUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenEmailIsEmpty()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Email = string.Empty,
            Name = "Test User",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        // Act
        TestValidationResult<RegisterUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenEmailIsInvalid()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Email = "invalid-email",
            Name = "Test User",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        // Act
        TestValidationResult<RegisterUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenEmailExceedsMaxLength()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Email = $"{new string('a', 255)}@test.com",
            Name = "Test User",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        // Act
        TestValidationResult<RegisterUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenNameIsEmpty()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Email = "test@example.com",
            Name = string.Empty,
            Password = "password123",
            ConfirmPassword = "password123"
        };

        // Act
        TestValidationResult<RegisterUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenNameIsTooShort()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Email = "test@example.com",
            Name = "a",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        // Act
        TestValidationResult<RegisterUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Email = "test@example.com",
            Name = new string('a', 101),
            Password = "password123",
            ConfirmPassword = "password123"
        };

        // Act
        TestValidationResult<RegisterUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenPasswordIsEmpty()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = string.Empty,
            ConfirmPassword = string.Empty
        };

        // Act
        TestValidationResult<RegisterUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenPasswordIsTooShort()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "12345",
            ConfirmPassword = "12345"
        };

        // Act
        TestValidationResult<RegisterUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenPasswordExceedsMaxLength()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = new string('a', 101),
            ConfirmPassword = new string('a', 101)
        };

        // Act
        TestValidationResult<RegisterUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "password123",
            ConfirmPassword = "differentpassword"
        };

        // Act
        TestValidationResult<RegisterUserDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }
}
