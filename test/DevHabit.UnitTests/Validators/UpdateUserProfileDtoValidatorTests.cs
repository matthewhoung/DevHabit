using DevHabit.Api.DTOs.Users;
using FluentValidation.TestHelper;

namespace DevHabit.UnitTests.Validators;

public sealed class UpdateUserProfileDtoValidatorTests
{
    private readonly UpdateUserProfileDtoValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldNotReturnError_WhenNameIsValid()
    {
        // Arrange
        var dto = new UpdateUserProfileDto("John Doe");

        // Act
        TestValidationResult<UpdateUserProfileDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_ShouldReturnError_WhenNameIsInvalid(string? name)
    {
        // Arrange
        var dto = new UpdateUserProfileDto(name!);

        // Act
        TestValidationResult<UpdateUserProfileDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenNameIsTooShort()
    {
        // Arrange
        var dto = new UpdateUserProfileDto("a");

        // Act
        TestValidationResult<UpdateUserProfileDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var dto = new UpdateUserProfileDto(new string('a', 101));

        // Act
        TestValidationResult<UpdateUserProfileDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
} 
