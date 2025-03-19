using DevHabit.Api.DTOs.Tags;
using FluentValidation.TestHelper;

namespace DevHabit.UnitTests.Validators;

public sealed class UpdateTagDtoValidatorTests
{
    private readonly UpdateTagDtoValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldReturnError_WhenNameIsEmpty()
    {
        // Arrange
        var dto = new UpdateTagDto
        {
            Name = string.Empty,
            Description = "Test description"
        };

        // Act
        TestValidationResult<UpdateTagDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenNameIsTooShort()
    {
        // Arrange
        var dto = new UpdateTagDto
        {
            Name = "ab",
            Description = "Test description"
        };

        // Act
        TestValidationResult<UpdateTagDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var dto = new UpdateTagDto
        {
            Name = new string('a', 51),
            Description = "Test description"
        };

        // Act
        TestValidationResult<UpdateTagDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var dto = new UpdateTagDto
        {
            Name = "Valid Name",
            Description = new string('a', 101)
        };

        // Act
        TestValidationResult<UpdateTagDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Validate_ShouldNotReturnError_WhenAllPropertiesAreValid()
    {
        // Arrange
        var dto = new UpdateTagDto
        {
            Name = "Valid Name",
            Description = "Valid description"
        };

        // Act
        TestValidationResult<UpdateTagDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_ShouldNotReturnError_WhenDescriptionIsNull()
    {
        // Arrange
        var dto = new UpdateTagDto
        {
            Name = "Valid Name",
            Description = null
        };

        // Act
        TestValidationResult<UpdateTagDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
