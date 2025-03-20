using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Entries;
using DevHabit.Api.DTOs.EntryImports;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.FunctionalTests.Infrastructure;
using System.Net.Http.Json;
using System.Net;
using System.Text;

namespace DevHabit.FunctionalTests.Tests;
public sealed class EntryImportFlowTests(DevHabitWebAppFactory factory) : FunctionalTestFixture(factory)
{
    [Fact]
    public async Task CompleteEntryImportFlow_ShouldSucceed()
    {
        // Arrange
        await CleanupDatabaseAsync();
        const string email = "importflow@test.com";
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

        // Step 4: Create CSV content for import
        string csvContent = $"""
            habit_id,date,value,notes
            {createdHabit.Id},2024-01-01,30,First day of reading
            {createdHabit.Id},2024-01-02,25,Second day of reading
            {createdHabit.Id},2024-01-03,35,Third day of reading
            """;

        // Step 5: Create and submit import job
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent));
        fileContent.Headers.ContentType = new("text/csv");
        content.Add(fileContent, "file", "entries.csv");

        HttpResponseMessage importResponse = await client.PostAsync(Routes.EntryImports.Create, content);
        Assert.Equal(HttpStatusCode.Created, importResponse.StatusCode);
        EntryImportJobDto? importJob = await importResponse.Content.ReadFromJsonAsync<EntryImportJobDto>();
        Assert.NotNull(importJob);

        // Step 6: Wait for import job to complete (with timeout)
        const int maxAttempts = 10;
        const int delayMs = 500;
        EntryImportJobDto? completedJob = null;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            await Task.Delay(delayMs);

            HttpResponseMessage jobStatusResponse = await client.GetAsync(Routes.EntryImports.GetById(importJob.Id));
            Assert.Equal(HttpStatusCode.OK, jobStatusResponse.StatusCode);

            completedJob = await jobStatusResponse.Content.ReadFromJsonAsync<EntryImportJobDto>();
            Assert.NotNull(completedJob);

            if (completedJob.Status is EntryImportStatus.Completed or EntryImportStatus.Failed)
            {
                break;
            }
        }

        Assert.NotNull(completedJob);
        Assert.Equal(EntryImportStatus.Completed, completedJob.Status);
        Assert.Equal(3, completedJob.ProcessedRecords);
        Assert.Equal(0, completedJob.FailedRecords);

        // Step 7: Verify imported entries
        HttpResponseMessage getEntriesResponse = await client.GetAsync(
            $"{Routes.Entries.GetAll}?habitId={createdHabit.Id}");
        Assert.Equal(HttpStatusCode.OK, getEntriesResponse.StatusCode);
        PaginationResult<EntryDto>? entries = await getEntriesResponse.Content
            .ReadFromJsonAsync<PaginationResult<EntryDto>>();
        Assert.NotNull(entries);
        Assert.Equal(3, entries.Items.Count);
    }
}
