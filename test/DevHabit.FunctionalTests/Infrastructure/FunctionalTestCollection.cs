namespace DevHabit.FunctionalTests.Infrastructure;

[CollectionDefinition(nameof(FunctionalTestCollection))]
public sealed class FunctionalTestCollection : ICollectionFixture<DevHabitWebAppFactory>;
