namespace GYM.IntegrationTests.Infrastructure;

[CollectionDefinition("Integration Tests")]
public sealed class IntegrationTestCollection
    : ICollectionFixture<IntegrationTestFixture>
{
}