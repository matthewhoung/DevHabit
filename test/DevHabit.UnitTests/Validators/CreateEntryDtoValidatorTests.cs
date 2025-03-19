using DevHabit.Api.DTOs.Entries;
using DevHabit.Api.Entities;
using FluentValidation.TestHelper;

namespace DevHabit.UnitTests.Validators;

public sealed class CreateEntryDtoValidatorTests
{
    private readonly CreateEntryDtoValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldNotReturnError_WhenAllPropertiesAreValid()
    {
        // Arrange
        var dto = new CreateEntryDto
        {
            HabitId = Habit.NewId(),
            Value = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // Act
        TestValidationResult<CreateEntryDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenHabitIdIsEmpty()
    {
        // Arrange
        var dto = new CreateEntryDto
        {
            HabitId = string.Empty,
            Value = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // Act
        TestValidationResult<CreateEntryDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HabitId);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenValueIsZero()
    {
        // Arrange
        var dto = new CreateEntryDto
        {
            HabitId = Habit.NewId(),
            Value = 0,
            Date = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // Act
        TestValidationResult<CreateEntryDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenValueIsNegative()
    {
        // Arrange
        var dto = new CreateEntryDto
        {
            HabitId = Habit.NewId(),
            Value = -1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // Act
        TestValidationResult<CreateEntryDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenDateIsDefault()
    {
        // Arrange
        var dto = new CreateEntryDto
        {
            HabitId = Habit.NewId(),
            Value = 1,
            Date = default
        };

        // Act
        TestValidationResult<CreateEntryDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public async Task Validate_ShouldNotReturnError_WhenNotesIsNull()
    {
        // Arrange
        var dto = new CreateEntryDto
        {
            HabitId = Habit.NewId(),
            Value = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Notes = null
        };

        // Act
        TestValidationResult<CreateEntryDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenNotesExceedsMaxLength()
    {
        // Arrange
        var dto = new CreateEntryDto
        {
            HabitId = Habit.NewId(),
            Value = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Notes = new string('a', 1001)
        };

        // Act
        TestValidationResult<CreateEntryDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }
}
