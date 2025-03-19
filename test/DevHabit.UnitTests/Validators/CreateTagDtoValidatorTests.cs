using DevHabit.Api.DTOs.Tags;
using FluentValidation.TestHelper;

namespace DevHabit.UnitTests.Validators;

public sealed class CreateTagDtoValidatorTests
{
    private readonly CreateTagDtoValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldNotReturnError_WhenNameIsValid()
    {
        // Arrange
        var dto = new CreateTagDto
        {
            Name = "Work"
        };

        // Act
        TestValidationResult<CreateTagDto>? result = await _validator.TestValidateAsync(dto);

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
        var dto = new CreateTagDto
        {
            Name = name!
        };

        // Act
        TestValidationResult<CreateTagDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var dto = new CreateTagDto
        {
            Name = new string('a', 51)
        };

        // Act
        TestValidationResult<CreateTagDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
