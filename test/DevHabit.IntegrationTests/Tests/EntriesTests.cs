using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Entries;
using DevHabit.Api.DTOs.Habits;
using DevHabit.IntegrationTests.Infrastructure;

namespace DevHabit.IntegrationTests.Tests;

public sealed class EntriesTests(DevHabitWebAppFactory factory) : IntegrationTestFixture(factory)
{
    [Fact]
    public async Task GetEntries_ShouldReturnEmptyList_WhenNoEntriesExist()
    {
        // Arrange
        await CleanupDatabaseAsync();

        HttpClient client = await CreateAuthenticatedClientAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Entries.GetAll);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginationResult<EntryDto>? result = await response.Content.ReadFromJsonAsync<PaginationResult<EntryDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetEntries_ShouldReturnEntries_WhenEntriesExist()
    {
        // Arrange
        await CleanupDatabaseAsync();

        HttpClient client = await CreateAuthenticatedClientAsync();

        // Create a habit first
        CreateHabitDto habitDto = TestData.Habits.CreateReadingHabit();
        HttpResponseMessage habitResponse = await client.PostAsJsonAsync(Routes.Habits.Create, habitDto);
        HabitDto? habit = await habitResponse.Content.ReadFromJsonAsync<HabitDto>();
        Assert.NotNull(habit);

        // Create an entry
        CreateEntryDto entryDto = TestData.Entries.CreateEntry(habit.Id);
        await client.PostAsJsonAsync(Routes.Entries.Create, entryDto);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Entries.GetAll);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginationResult<EntryDto>? result = await response.Content.ReadFromJsonAsync<PaginationResult<EntryDto>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(entryDto.Value, result.Items[0].Value);
    }

    [Fact]
    public async Task GetEntries_ShouldFilterByHabitId()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Create two habits
        CreateHabitDto habit1Dto = TestData.Habits.CreateReadingHabit();
        CreateHabitDto habit2Dto = TestData.Habits.CreateExerciseHabit();

        HttpResponseMessage habit1Response = await client.PostAsJsonAsync(Routes.Habits.Create, habit1Dto);
        HttpResponseMessage habit2Response = await client.PostAsJsonAsync(Routes.Habits.Create, habit2Dto);
        
        HabitDto? habit1 = await habit1Response.Content.ReadFromJsonAsync<HabitDto>();
        HabitDto? habit2 = await habit2Response.Content.ReadFromJsonAsync<HabitDto>();
        Assert.NotNull(habit1);
        Assert.NotNull(habit2);

        // Create entries for both habits
        CreateEntryDto entry1Dto = TestData.Entries.CreateEntry(habit1.Id, note: "Reading entry");
        CreateEntryDto entry2Dto = TestData.Entries.CreateEntry(habit2.Id, value: 20, note: "Exercise entry");
        await client.PostAsJsonAsync(Routes.Entries.Create, entry1Dto);
        await client.PostAsJsonAsync(Routes.Entries.Create, entry2Dto);

        // Act
        HttpResponseMessage response = await client.GetAsync($"{Routes.Entries.GetAll}?habitId={habit1.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginationResult<EntryDto>? result = await response.Content.ReadFromJsonAsync<PaginationResult<EntryDto>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(entry1Dto.Value, result.Items[0].Value);
    }

    [Fact]
    public async Task GetEntries_ShouldFilterByDateRange()
    {
        // Arrange
        await CleanupDatabaseAsync();

        HttpClient client = await CreateAuthenticatedClientAsync();

        // Create a habit
        CreateHabitDto habitDto = TestData.Habits.CreateReadingHabit();
        HttpResponseMessage habitResponse = await client.PostAsJsonAsync(Routes.Habits.Create, habitDto);
        HabitDto? habit = await habitResponse.Content.ReadFromJsonAsync<HabitDto>();
        Assert.NotNull(habit);

        // Create entries for different dates
        CreateEntryDto entry1Dto = TestData.Entries.CreateEntryForDate(
            habit.Id, 
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), 
            note: "Yesterday's entry"
        );
        CreateEntryDto entry2Dto = TestData.Entries.CreateEntryForDate(
            habit.Id, 
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), 
            value: 20, 
            note: "Tomorrow's entry"
        );
        await client.PostAsJsonAsync(Routes.Entries.Create, entry1Dto);
        await client.PostAsJsonAsync(Routes.Entries.Create, entry2Dto);

        // Act
        string fromDate = DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        string toDate = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        HttpResponseMessage response = await client.GetAsync(
            $"{Routes.Entries.GetAll}?fromDate={fromDate}&toDate={toDate}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PaginationResult<EntryDto>? result = await response.Content.ReadFromJsonAsync<PaginationResult<EntryDto>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(entry1Dto.Value, result.Items[0].Value);
    }

    [Fact]
    public async Task CreateEntry_ShouldSucceed_WithValidParameters()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Create a habit first
        CreateHabitDto habitDto = TestData.Habits.CreateReadingHabit();
        HttpResponseMessage habitResponse = await client.PostAsJsonAsync(Routes.Habits.Create, habitDto);
        HabitDto? habit = await habitResponse.Content.ReadFromJsonAsync<HabitDto>();
        Assert.NotNull(habit);

        CreateEntryDto entryDto = TestData.Entries.CreateEntry(habit.Id);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Entries.Create, entryDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        EntryDto? result = await response.Content.ReadFromJsonAsync<EntryDto>();
        Assert.NotNull(result);
        Assert.Equal(entryDto.Value, result.Value);
    }

    [Fact]
    public async Task UpdateEntry_ShouldSucceed_WithValidParameters()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Create a habit first
        CreateHabitDto habitDto = TestData.Habits.CreateReadingHabit();
        HttpResponseMessage habitResponse = await client.PostAsJsonAsync(Routes.Habits.Create, habitDto);
        HabitDto? habit = await habitResponse.Content.ReadFromJsonAsync<HabitDto>();
        Assert.NotNull(habit);

        // Create an entry
        CreateEntryDto createDto = TestData.Entries.CreateEntry(habit.Id);
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(Routes.Entries.Create, createDto);
        EntryDto? createdEntry = await createResponse.Content.ReadFromJsonAsync<EntryDto>();
        Assert.NotNull(createdEntry);

        // Update the entry
        UpdateEntryDto updateDto = TestData.Entries.CreateUpdateEntry();

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(Routes.Entries.Update(createdEntry.Id), updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the update
        HttpResponseMessage getResponse = await client.GetAsync(Routes.Entries.GetById(createdEntry.Id));
        EntryDto? updatedEntry = await getResponse.Content.ReadFromJsonAsync<EntryDto>();
        Assert.NotNull(updatedEntry);
        Assert.Equal(updateDto.Value, updatedEntry.Value);
        Assert.Equal(updateDto.Notes, updatedEntry.Notes);
    }

    [Fact]
    public async Task DeleteEntry_ShouldSucceed_WhenEntryExists()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Create a habit first
        CreateHabitDto habitDto = TestData.Habits.CreateReadingHabit();
        HttpResponseMessage habitResponse = await client.PostAsJsonAsync(Routes.Habits.Create, habitDto);
        HabitDto? habit = await habitResponse.Content.ReadFromJsonAsync<HabitDto>();
        Assert.NotNull(habit);

        // Create an entry
        CreateEntryDto createDto = TestData.Entries.CreateEntry(habit.Id);
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(Routes.Entries.Create, createDto);
        EntryDto? createdEntry = await createResponse.Content.ReadFromJsonAsync<EntryDto>();
        Assert.NotNull(createdEntry);

        // Act
        HttpResponseMessage response = await client.DeleteAsync(Routes.Entries.Delete(createdEntry.Id));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the deletion
        HttpResponseMessage getResponse = await client.GetAsync(Routes.Entries.GetById(createdEntry.Id));
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateBatch_ShouldSucceed_WithValidParameters()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Create a habit first
        CreateHabitDto habitDto = TestData.Habits.CreateReadingHabit();
        HttpResponseMessage habitResponse = await client.PostAsJsonAsync(Routes.Habits.Create, habitDto);
        HabitDto? habit = await habitResponse.Content.ReadFromJsonAsync<HabitDto>();
        Assert.NotNull(habit);

        // Create batch entries
        CreateEntryBatchDto batchDto = TestData.Entries.CreateBatch(
            habit.Id,
            (DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), 10),
            (DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), 20),
            (DateOnly.FromDateTime(DateTime.UtcNow), 30)
        );

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Entries.CreateBatch, batchDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        List<EntryDto>? result = await response.Content.ReadFromJsonAsync<List<EntryDto>>();
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(batchDto.Entries[0].Value, result[0].Value);
        Assert.Equal(batchDto.Entries[1].Value, result[1].Value);
        Assert.Equal(batchDto.Entries[2].Value, result[2].Value);
    }

    [Fact]
    public async Task GetStats_ShouldReturnStats_WhenEntriesExist()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Create a habit first
        CreateHabitDto habitDto = TestData.Habits.CreateReadingHabit();
        HttpResponseMessage habitResponse = await client.PostAsJsonAsync(Routes.Habits.Create, habitDto);
        HabitDto? habit = await habitResponse.Content.ReadFromJsonAsync<HabitDto>();
        Assert.NotNull(habit);

        // Create entries
        CreateEntryDto[] entries =
        [
            TestData.Entries.CreateEntryForDate(habit.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), 10),
            TestData.Entries.CreateEntryForDate(habit.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), 20),
            TestData.Entries.CreateEntryForDate(habit.Id, DateOnly.FromDateTime(DateTime.UtcNow), 30)
        ];

        foreach (CreateEntryDto entry in entries)
        {
            await client.PostAsJsonAsync(Routes.Entries.Create, entry);
        }

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Entries.Stats);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        EntryStatsDto? stats = await response.Content.ReadFromJsonAsync<EntryStatsDto>();
        Assert.NotNull(stats);
    }
} 
