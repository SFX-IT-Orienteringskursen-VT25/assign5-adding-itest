using Microsoft.AspNetCore.Mvc.Testing;
using SetupMssqlExample;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await DockerStarter.StartDockerContainerAsync();
        Database.Setup();
        Database.DeleteAll();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Database.DeleteAll();
        await base.DisposeAsync();
    }
}