// tests/IntegrationTests/IntegrationTestsCollection.cs
using Xunit;

namespace IntegrationTests;

[CollectionDefinition("IntegrationTests collection")]
public class IntegrationTestsCollection : ICollectionFixture<SqlTestContainerFixture>
{
    // Empty class: Associates a fixture with a collection
}
