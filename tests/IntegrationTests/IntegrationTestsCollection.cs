// tests/IntegrationTests/IntegrationTestsCollection.cs
using Xunit;

namespace IntegrationTests;

[CollectionDefinition("IntegrationTests collection")]
public class IntegrationTestsCollection : ICollectionFixture<SqlTestContainerFixture>
{
    // Пустой класс: связывает фикстуру с коллекцией
}
