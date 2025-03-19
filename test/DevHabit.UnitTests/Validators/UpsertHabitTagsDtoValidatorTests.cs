using DevHabit.Api.DTOs.HabitTags;
using DevHabit.Api.Entities;
using FluentValidation.TestHelper;

namespace DevHabit.UnitTests.Validators;

public sealed class UpsertHabitTagsDtoValidatorTests
{
    private readonly UpsertHabitTagsDtoValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldNotReturnError_WhenTagIdsAreValid()
    {
        // Arrange
        var dto = new UpsertHabitTagsDto
        {
            TagIds = [Tag.NewId(), Tag.NewId()]
        };

        // Act
        TestValidationResult<UpsertHabitTagsDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenTagIdsAreEmpty()
    {
        // Arrange
        var dto = new UpsertHabitTagsDto
        {
            TagIds = []
        };

        // Act
        TestValidationResult<UpsertHabitTagsDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task Validate_ShouldReturnError_WhenTagIdsContainDuplicates()
    {
        // Arrange
        string tagId = Tag.NewId();
        var dto = new UpsertHabitTagsDto
        {
            TagIds = [tagId, tagId]
        };

        // Act
        TestValidationResult<UpsertHabitTagsDto>? result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TagIds);
    }
}
