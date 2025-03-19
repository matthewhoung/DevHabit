using System.Net;
using System.Net.Http.Json;
using DevHabit.Api.DTOs.Tags;
using DevHabit.Api.Settings;
using DevHabit.Api.Services;
using DevHabit.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DevHabit.IntegrationTests.Tests;

public sealed class TagsTests(DevHabitWebAppFactory factory) : IntegrationTestFixture(factory)
{
    [Fact]
    public async Task GetTags_ShouldReturnEmptyList_WhenNoTagsExist()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Tags.GetAll);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        TagsCollectionDto? result = await response.Content.ReadFromJsonAsync<TagsCollectionDto>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetTags_ShouldReturnTags_WhenTagsExist()
    {
        // Arrange
        await CleanupDatabaseAsync();

        HttpClient client = await CreateAuthenticatedClientAsync();

        // Create a tag first
        CreateTagDto createDto = TestData.Tags.CreateImportantTag();
        await client.PostAsJsonAsync(Routes.Tags.Create, createDto);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Tags.GetAll);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        TagsCollectionDto? result = await response.Content.ReadFromJsonAsync<TagsCollectionDto>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(createDto.Name, result.Items[0].Name);
    }

    [Fact]
    public async Task GetTags_ShouldIncludeHateoasLinks_WhenRequested()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();
        client.DefaultRequestHeaders.Accept.Add(new(CustomMediaTypeNames.Application.HateoasJson));

        // Create a tag first
        CreateTagDto createDto = TestData.Tags.CreateImportantTag();
        await client.PostAsJsonAsync(Routes.Tags.Create, createDto);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.Tags.GetAll);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        TagsCollectionDto? result = await response.Content.ReadFromJsonAsync<TagsCollectionDto>();
        Assert.NotNull(result);
        Assert.NotNull(result.Links);
        Assert.NotEmpty(result.Links);
        Assert.NotNull(result.Items[0].Links);
        Assert.NotEmpty(result.Items[0].Links);
    }

    [Fact]
    public async Task CreateTag_ShouldSucceed_WithValidParameters()
    {
        // Arrange
        CreateTagDto dto = TestData.Tags.CreateImportantTag();
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Tags.Create, dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        TagDto? result = await response.Content.ReadFromJsonAsync<TagDto>();
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
    }

    [Fact]
    public async Task CreateTag_ShouldFail_WhenMaxTagsLimitReached()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Get the max tags limit from configuration
        TagsOptions tagsOptions = factory.Services.GetRequiredService<IOptions<TagsOptions>>().Value;

        // Create max allowed tags
        for (int i = 1; i <= tagsOptions.MaxAllowedTags; i++)
        {
            var dto = new CreateTagDto { Name = $"Test Tag {i}" };
            await client.PostAsJsonAsync(Routes.Tags.Create, dto);
        }

        // Try to create one more
        var extraDto = new CreateTagDto { Name = "Extra Tag" };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Tags.Create, extraDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTag_ShouldSucceed_WithValidParameters()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Create a tag first
        CreateTagDto createDto = TestData.Tags.CreateImportantTag();
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(Routes.Tags.Create, createDto);
        TagDto? createdTag = await createResponse.Content.ReadFromJsonAsync<TagDto>();
        Assert.NotNull(createdTag);

        // Update the tag
        var updateDto = new UpdateTagDto { Name = "Updated Tag" };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(Routes.Tags.Update(createdTag.Id), updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the update
        HttpResponseMessage getResponse = await client.GetAsync(Routes.Tags.GetById(createdTag.Id));
        TagDto? updatedTag = await getResponse.Content.ReadFromJsonAsync<TagDto>();
        Assert.NotNull(updatedTag);
        Assert.Equal(updateDto.Name, updatedTag.Name);
    }

    [Fact]
    public async Task DeleteTag_ShouldSucceed_WhenTagExists()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Create a tag first
        CreateTagDto createDto = TestData.Tags.CreateImportantTag();
        HttpResponseMessage createResponse = await client.PostAsJsonAsync(Routes.Tags.Create, createDto);
        TagDto? createdTag = await createResponse.Content.ReadFromJsonAsync<TagDto>();
        Assert.NotNull(createdTag);

        // Act
        HttpResponseMessage response = await client.DeleteAsync(Routes.Tags.Delete(createdTag.Id));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the deletion
        HttpResponseMessage getResponse = await client.GetAsync(Routes.Tags.GetById(createdTag.Id));
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetTags_ShouldUseCaching_WhenEnabled()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();

        // First request
        HttpResponseMessage response1 = await client.GetAsync(Routes.Tags.GetAll);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        // Second request should hit the cache
        HttpResponseMessage response2 = await client.GetAsync(Routes.Tags.GetAll);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        // Verify cache headers
        Assert.NotNull(response2.Headers.CacheControl);
        Assert.True(response2.Headers.CacheControl.MaxAge.HasValue);
        Assert.Equal(TimeSpan.FromSeconds(120), response2.Headers.CacheControl.MaxAge);
    }
}
