using DevHabit.Api.DTOs.Entries;
using FluentValidation.TestHelper;

namespace DevHabit.UnitTests.Validators;

public sealed class UpdateEntryDtoValidatorTests
{
    private readonly UpdateEntryDtoValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldNotReturnError_WhenAllPropertiesAreValid()
    {
        // Arrange
        var dto = new UpdateEntryDto
        {
            Value = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Notes = "Test notes"
        };

        // Act
        TestValidationResult<UpdateEntryDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_ShouldReturnError_WhenValueIsNegative(int value)
    {
        // Arrange
        var dto = new UpdateEntryDto
        {
            Value = value,
            Date = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // Act
        TestValidationResult<UpdateEntryDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenNotesExceedMaxLength()
    {
        // Arrange
        var dto = new UpdateEntryDto
        {
            Value = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Notes = new string('a', 1001)
        };

        // Act
        TestValidationResult<UpdateEntryDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public async Task Validate_ShouldNotReturnError_WhenNotesIsNull()
    {
        // Arrange
        var dto = new UpdateEntryDto
        {
            Value = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Notes = null
        };

        // Act
        TestValidationResult<UpdateEntryDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenDateIsInFuture()
    {
        // Arrange
        var dto = new UpdateEntryDto
        {
            Value = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
        };

        // Act
        TestValidationResult<UpdateEntryDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Date);
    }
} 