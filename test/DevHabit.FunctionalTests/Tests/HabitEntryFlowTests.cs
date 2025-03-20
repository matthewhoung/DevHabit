using System.Net.Http.Json;
using System.Net;
using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Entries;
using DevHabit.Api.DTOs.Habits;
using DevHabit.FunctionalTests.Infrastructure;

namespace DevHabit.FunctionalTests.Tests;
public sealed class HabitEntryFlowTests(DevHabitWebAppFactory factory) : FunctionalTestFixture(factory)
{
    [Fact]
    public async Task CompleteHabitEntryFlow_ShouldSucceed()
    {
        // Arrange
        await CleanupDatabaseAsync();
        const string email = "entryflow@test.com";
        const string password = "Test123!";

        // Step 1: Register a new user
        HttpClient client = CreateClient();
        var registerDto = new RegisterUserDto
        {
            Name = email,
            Email = email,
            Password = password,
            ConfirmPassword = password
        };
        HttpResponseMessage registerResponse = await client.PostAsJsonAsync(Routes.Auth.Register, registerDto);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // Step 2: Login to get the token
        var loginDto = new LoginUserDto
        {
            Email = email,
            Password = password
        };
        HttpResponseMessage loginResponse = await client.PostAsJsonAsync(Routes.Auth.Login, loginDto);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        AccessTokensDto? tokens = await loginResponse.Content.ReadFromJsonAsync<AccessTokensDto>();
        Assert.NotNull(tokens);
        client.DefaultRequestHeaders.Authorization = new("Bearer", tokens.AccessToken);

        // Step 3: Create a habit
        CreateHabitDto habitDto = TestData.Habits.CreateReadingHabit();
        HttpResponseMessage createHabitResponse = await client.PostAsJsonAsync(Routes.Habits.Create, habitDto);
        Assert.Equal(HttpStatusCode.Created, createHabitResponse.StatusCode);
        HabitDto? createdHabit = await createHabitResponse.Content.ReadFromJsonAsync<HabitDto>();
        Assert.NotNull(createdHabit);

        // Step 4: Create first entry
        CreateEntryDto firstEntryDto = TestData.Entries.CreateEntry(
            createdHabit.Id,
            value: 25,
            note: "First reading session");
        HttpResponseMessage firstEntryResponse = await client.PostAsJsonAsync(Routes.Entries.Create, firstEntryDto);
        Assert.Equal(HttpStatusCode.Created, firstEntryResponse.StatusCode);
        EntryDto? firstEntry = await firstEntryResponse.Content.ReadFromJsonAsync<EntryDto>();
        Assert.NotNull(firstEntry);
        Assert.Equal(25, firstEntry.Value);

        // Step 5: Create second entry for the next day
        CreateEntryDto secondEntryDto = TestData.Entries.CreateEntryForDate(
            createdHabit.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            value: 35,
            note: "Second reading session"
        );
        HttpResponseMessage secondEntryResponse = await client.PostAsJsonAsync(Routes.Entries.Create, secondEntryDto);
        Assert.Equal(HttpStatusCode.Created, secondEntryResponse.StatusCode);
        EntryDto? secondEntry = await secondEntryResponse.Content.ReadFromJsonAsync<EntryDto>();
        Assert.NotNull(secondEntry);
        Assert.Equal(35, secondEntry.Value);

        // Step 6: Get all entries and verify
        HttpResponseMessage getEntriesResponse = await client.GetAsync(
            $"{Routes.Entries.GetAll}?habitId={createdHabit.Id}");
        Assert.Equal(HttpStatusCode.OK, getEntriesResponse.StatusCode);
        PaginationResult<EntryDto>? entries = await getEntriesResponse.Content
            .ReadFromJsonAsync<PaginationResult<EntryDto>>();
        Assert.NotNull(entries);
        Assert.Equal(2, entries.Items.Count);

        // Step 7: Get entry statistics
        HttpResponseMessage getStatsResponse = await client.GetAsync(Routes.Entries.Stats);
        Assert.Equal(HttpStatusCode.OK, getStatsResponse.StatusCode);
        EntryStatsDto? stats = await getStatsResponse.Content.ReadFromJsonAsync<EntryStatsDto>();
        Assert.NotNull(stats);
        Assert.True(stats.TotalEntries >= 2);
    }
}
