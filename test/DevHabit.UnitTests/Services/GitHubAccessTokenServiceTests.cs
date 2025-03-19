using System.Security.Cryptography;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.GitHub;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DevHabit.UnitTests.Services;

public sealed class GitHubAccessTokenServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly EncryptionService _encryptionService;
    private readonly GitHubAccessTokenService _gitHubAccessTokenService;

    public GitHubAccessTokenServiceTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        IOptions<EncryptionOptions> encryptionOptions = Options.Create(new EncryptionOptions
        {
            Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        });

        _encryptionService = new EncryptionService(encryptionOptions);
        _gitHubAccessTokenService = new GitHubAccessTokenService(_dbContext, _encryptionService);
    }

    [Fact]
    public async Task StoreAsync_ShouldCreateNewToken_WhenUserDoesNotHaveOne()
    {
        // Arrange
        const string userId = "user123";
        var dto = new StoreGitHubAccessTokenDto
        {
            AccessToken = "github-token",
            ExpiresInDays = 30
        };

        // Act
        await _gitHubAccessTokenService.StoreAsync(userId, dto);

        // Assert
        GitHubAccessToken? token = await _dbContext.GitHubAccessTokens.SingleOrDefaultAsync(t => t.UserId == userId);
        Assert.NotNull(token);
        Assert.Equal(userId, token.UserId);
        Assert.NotEqual(dto.AccessToken, token.Token); // Token should be encrypted
        Assert.True(token.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task StoreAsync_ShouldUpdateExistingToken_WhenUserHasOne()
    {
        // Arrange
        const string userId = "user123";
        var existingToken = new GitHubAccessToken
        {
            Id = GitHubAccessToken.NewId(),
            UserId = userId,
            Token = _encryptionService.Encrypt("old-token"),
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(29)
        };
        await _dbContext.GitHubAccessTokens.AddAsync(existingToken);
        await _dbContext.SaveChangesAsync();

        var dto = new StoreGitHubAccessTokenDto
        {
            AccessToken = "new-token",
            ExpiresInDays = 30
        };

        // Act
        _dbContext.ChangeTracker.Clear(); // Because of in-memory database...
        await _gitHubAccessTokenService.StoreAsync(userId, dto);

        // Assert
        GitHubAccessToken? token = await _dbContext.GitHubAccessTokens.SingleOrDefaultAsync(t => t.UserId == userId);
        Assert.NotNull(token);
        Assert.Equal(existingToken.Id, token.Id);
        Assert.NotEqual(existingToken.Token, token.Token);
        Assert.True(token.ExpiresAtUtc > existingToken.ExpiresAtUtc);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDecryptedToken_WhenTokenExists()
    {
        // Arrange
        const string userId = "user123";
        const string originalToken = "github-token";
        var existingToken = new GitHubAccessToken
        {
            Id = GitHubAccessToken.NewId(),
            UserId = userId,
            Token = _encryptionService.Encrypt(originalToken),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
        };
        await _dbContext.GitHubAccessTokens.AddAsync(existingToken);
        await _dbContext.SaveChangesAsync();

        // Act
        string? result = await _gitHubAccessTokenService.GetAsync(userId);

        // Assert
        Assert.Equal(originalToken, result);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenTokenDoesNotExist()
    {
        // Arrange
        const string userId = "user123";

        // Act
        string? result = await _gitHubAccessTokenService.GetAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RevokeAsync_ShouldRemoveToken_WhenTokenExists()
    {
        // Arrange
        const string userId = "user123";
        var existingToken = new GitHubAccessToken
        {
            Id = GitHubAccessToken.NewId(),
            UserId = userId,
            Token = _encryptionService.Encrypt("github-token"),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
        };
        await _dbContext.GitHubAccessTokens.AddAsync(existingToken);
        await _dbContext.SaveChangesAsync();

        // Act
        await _gitHubAccessTokenService.RevokeAsync(userId);

        // Assert
        Assert.False(await _dbContext.GitHubAccessTokens.AnyAsync(t => t.UserId == userId));
    }

    [Fact]
    public async Task RevokeAsync_ShouldNotThrow_WhenTokenDoesNotExist()
    {
        // Arrange
        const string userId = "user123";

        // Act & Assert
        await _gitHubAccessTokenService.RevokeAsync(userId);
        Assert.False(await _dbContext.GitHubAccessTokens.AnyAsync(t => t.UserId == userId));
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
