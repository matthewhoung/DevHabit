using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.DTOs.HabitTags;
using DevHabit.Api.DTOs.Tags;
using DevHabit.FunctionalTests.Infrastructure;
using System.Net.Http.Json;
using System.Net;

namespace DevHabit.FunctionalTests.Tests;
public sealed class HabitManagementFlowTests(DevHabitWebAppFactory factory) : FunctionalTestFixture(factory)
{
    [Fact]
    public async Task CompleteHabitManagementFlow_ShouldSucceed()
    {
        // Arrange
        await CleanupDatabaseAsync();
        const string email = "habitflow@test.com";
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
        Assert.Equal(habitDto.Name, createdHabit.Name);

        // Step 4: Create a tag
        CreateTagDto tagDto = TestData.Tags.CreateProductivityTag();
        HttpResponseMessage createTagResponse = await client.PostAsJsonAsync(Routes.Tags.Create, tagDto);
        Assert.Equal(HttpStatusCode.Created, createTagResponse.StatusCode);
        TagDto? createdTag = await createTagResponse.Content.ReadFromJsonAsync<TagDto>();
        Assert.NotNull(createdTag);
        Assert.Equal(tagDto.Name, createdTag.Name);

        // Step 5: Tag the habit
        UpsertHabitTagsDto upsertTagsDto = TestData.HabitTags.CreateUpsertDto(createdTag.Id);
        HttpResponseMessage tagHabitResponse = await client.PutAsJsonAsync(
            Routes.HabitTags.UpsertTags(createdHabit.Id),
            upsertTagsDto);
        Assert.Equal(HttpStatusCode.NoContent, tagHabitResponse.StatusCode);

        // Step 6: Get all habits and verify the tagged habit
        HttpResponseMessage getHabitsResponse = await client.GetAsync(Routes.Habits.GetById(createdHabit.Id));
        Assert.Equal(HttpStatusCode.OK, getHabitsResponse.StatusCode);
        HabitWithTagsDto? habitWithTags = await getHabitsResponse.Content.ReadFromJsonAsync<HabitWithTagsDto>();
        Assert.NotNull(habitWithTags);
        Assert.Single(habitWithTags.Tags);
        Assert.Equal(createdTag.Name, habitWithTags.Tags[0]);
    }
}
