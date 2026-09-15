using Testcontainers.MsSql;
using Xunit;

namespace AdditionApi.IntegrationTests;

public class SqlServerContainerTest
{
[Fact]
public async Task SqlServerContainer_Starts()
{
await using var sqlContainer = new MsSqlBuilder(
"mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
.Build();

    await sqlContainer.StartAsync();
}

}