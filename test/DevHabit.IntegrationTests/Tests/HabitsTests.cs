using System.Dynamic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

namespace DevHabit.IntegrationTests.Tests;

public sealed class HabitsTests(DevHabitWebAppFactory factory) : IntegrationTestFixture(factory)
{
    [Fact]
    public async Task GetHabits_ShouldReturnEmptyList_WhenNoHabitsExist()
    {
        // Arrange
        await CleanupDatabaseAsync();

        HttpClient client = await CreateAuthenticatedClientAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Habits.GetAll);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginationResult<HabitDto>? result = await response.Content.ReadFromJsonAsync<PaginationResult<HabitDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetHabits_ShouldReturnHabits_WhenHabitsExist()
    {
        // Arrange
        await CleanupDatabaseAsync();

        HttpClient client = await CreateAuthenticatedClientAsync();

        // Create a habit first
        CreateHabitDto createDto = TestData.Habits.CreateReadingHabit();
        await client.PostAsJsonAsync(Routes.Habits.Create, createDto);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Habits.GetAll);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginationResult<HabitDto>? result = await response.Content.ReadFromJsonAsync<PaginationResult<HabitDto>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(createDto.Name, result.Items[0].Name);
    }

    [Fact]
    public async Task GetHabits_ShouldSupportFiltering()
    {
        // Arrange
        await CleanupDatabaseAsync();

        HttpClient client = await CreateAuthenticatedClientAsync();

        // Create habits with different types
        CreateHabitDto measurableHabit = TestData.Habits.CreateReadingHabit();
        CreateHabitDto binaryHabit = TestData.Habits.CreateExerciseHabit();
        binaryHabit = binaryHabit with { Type = HabitType.Binary };

        await client.PostAsJsonAsync(Routes.Habits.Create, measurableHabit);
        await client.PostAsJsonAsync(Routes.Habits.Create, binaryHabit);

        // Act
        HttpResponseMessage response = await client.GetAsync(
            $"{Routes.Habits.GetAll}?type={(int)HabitType.Measurable}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginationResult<HabitDto>? result = await response.Content.ReadFromJsonAsync<PaginationResult<HabitDto>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(HabitType.Measurable, result.Items[0].Type);
    }

    [Fact]
    public async Task GetHabits_ShouldSupportSorting()
    {
        // Arrange
        await CleanupDatabaseAsync();

        HttpClient client = await CreateAuthenticatedClientAsync();

        // Create habits with different names
        CreateHabitDto[] habits =
        [
            new()
            {
                Name = "Z Habit",
                Type = HabitType.Measurable,
                Frequency = new FrequencyDto
                {
                    Type = FrequencyType.Daily,
                    TimesPerPeriod = 1
                },
                Target = new TargetDto
                {
                    Value = 30,
                    Unit = "pages"
                }
            },
            new()
            {
                Name = "A Habit",
                Type = HabitType.Measurable,
                Frequency = new FrequencyDto
                {
                    Type = FrequencyType.Daily,
                    TimesPerPeriod = 1
                },
                Target = new TargetDto
                {
                    Value = 30,
                    Unit = "pages"
                }
            }
        ];

        foreach (CreateHabitDto habit in habits)
        {
            await client.PostAsJsonAsync(Routes.Habits.Create, habit);
        }

        // Act
        HttpResponseMessage response = await client.GetAsync($"{Routes.Habits.GetAll}?sort=name");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginationResult<HabitDto>? result = await response.Content.ReadFromJsonAsync<PaginationResult<HabitDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("A Habit", result.Items[0].Name);
        Assert.Equal("Z Habit", result.Items[1].Name);
    }

    [Fact]
    public async Task GetHabits_ShouldSupportDataShaping()
    {
        // Arrange
        await CleanupDatabaseAsync();

        HttpClient client = await CreateAuthenticatedClientAsync();
        CreateHabitDto habit = TestData.Habits.CreateReadingHabit();
        await client.PostAsJsonAsync(Routes.Habits.Create, habit);

        // Act
        HttpResponseMessage response = await client.GetAsync($"{Routes.Habits.GetAll}?fields=name,type");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginationResult<ExpandoObject>? result = await response.Content
            .ReadFromJsonAsync<PaginationResult<ExpandoObject>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        
        dynamic item = result.Items[0];
        Assert.Equal(2, ((IDictionary<string, object>)item).Count);
        Assert.True(((IDictionary<string, object>)item).ContainsKey("name"));
        Assert.True(((IDictionary<string, object>)item).ContainsKey("type"));
    }

    [Fact]
    public async Task GetHabit_ShouldReturnHabit_WhenExists()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();
        CreateHabitDto createDto = TestData.Habits.CreateReadingHabit();
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(Routes.Habits.Create, createDto);
        HabitDto? createdHabit = await createResponse.Content.ReadFromJsonAsync<HabitDto>();
        Assert.NotNull(createdHabit);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Habits.GetById(createdHabit.Id));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        HabitWithTagsDto? habit = await response.Content.ReadFromJsonAsync<HabitWithTagsDto>();
        Assert.NotNull(habit);
        Assert.Equal(createDto.Name, habit.Name);
    }

    [Fact]
    public async Task GetHabit_ShouldSupportVersioning()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();
        CreateHabitDto createDto = TestData.Habits.CreateReadingHabit();
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(Routes.Habits.Create, createDto);
        HabitDto? createdHabit = await createResponse.Content.ReadFromJsonAsync<HabitDto>();
        Assert.NotNull(createdHabit);

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Add("Accept", CustomMediaTypeNames.Application.JsonV2);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Habits.GetById(createdHabit.Id));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateHabit_ShouldSucceed_WithValidParameters()
    {
        // Arrange
        CreateHabitDto dto = TestData.Habits.CreateReadingHabit();
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Habits.Create, dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.NotNull(await response.Content.ReadFromJsonAsync<HabitDto>());
    }

    [Fact]
    public async Task UpdateHabit_ShouldSucceed_WithValidParameters()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();
        CreateHabitDto createDto = TestData.Habits.CreateReadingHabit();
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(Routes.Habits.Create, createDto);
        HabitDto? createdHabit = await createResponse.Content.ReadFromJsonAsync<HabitDto>();
        Assert.NotNull(createdHabit);

        var updateDto = new UpdateHabitDto
        {
            Type = HabitType.Measurable,
            Name = "Updated Habit",
            Description = "Updated description",
            Frequency = new FrequencyDto
            {
                Type = FrequencyType.Weekly,
                TimesPerPeriod = 3
            },
            Target = new TargetDto
            {
                Value = 50,
                Unit = "pages"
            }
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(Routes.Habits.Update(createdHabit.Id), updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the update
        HttpResponseMessage getResponse = await client.GetAsync(Routes.Habits.GetById(createdHabit.Id));
        HabitWithTagsDto? updatedHabit = await getResponse.Content.ReadFromJsonAsync<HabitWithTagsDto>();
        Assert.NotNull(updatedHabit);
        Assert.Equal(updateDto.Name, updatedHabit.Name);
        Assert.Equal(updateDto.Description, updatedHabit.Description);
        Assert.Equal(updateDto.Frequency.Type, updatedHabit.Frequency.Type);
        Assert.Equal(updateDto.Target.Value, updatedHabit.Target.Value);
    }

    [Fact]
    public async Task PatchHabit_ShouldSucceed_WithValidParameters()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();
        CreateHabitDto createDto = TestData.Habits.CreateReadingHabit();
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(Routes.Habits.Create, createDto);
        HabitDto? createdHabit = await createResponse.Content.ReadFromJsonAsync<HabitDto>();
        Assert.NotNull(createdHabit);

        var patchDoc = new JsonPatchDocument<UpdateHabitDto>();
        patchDoc.Replace(x => x.Name, "Patched Habit Name");

        // Act
        using var stringContent = new StringContent(
            JsonConvert.SerializeObject(patchDoc),
            new MediaTypeHeaderValue(MediaTypeNames.Application.JsonPatch));
        HttpResponseMessage response = await client.PatchAsync(
            Routes.Habits.Patch(createdHabit.Id),
            stringContent);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the patch
        HttpResponseMessage getResponse = await client.GetAsync(Routes.Habits.GetById(createdHabit.Id));
        HabitWithTagsDto? patchedHabit = await getResponse.Content.ReadFromJsonAsync<HabitWithTagsDto>();
        Assert.NotNull(patchedHabit);
        Assert.Equal("Patched Habit Name", patchedHabit.Name);
    }

    [Fact]
    public async Task DeleteHabit_ShouldSucceed_WhenHabitExists()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();
        CreateHabitDto createDto = TestData.Habits.CreateReadingHabit();
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(Routes.Habits.Create, createDto);
        HabitDto? createdHabit = await createResponse.Content.ReadFromJsonAsync<HabitDto>();
        Assert.NotNull(createdHabit);

        // Act
        HttpResponseMessage response = await client.DeleteAsync(Routes.Habits.Delete(createdHabit.Id));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the deletion
        HttpResponseMessage getResponse = await client.GetAsync(Routes.Habits.GetById(createdHabit.Id));
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
