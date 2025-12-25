using Docker.DotNet;
using Docker.DotNet.Models;
namespace AdditionApi;

public class DockerStarter
{
    public static async Task StartDockerContainerAsync()
    {
        try
        {
            // Determine Docker socket URI based on OS and available sockets
            Uri dockerUri;
            if (OperatingSystem.IsWindows())
            {
                dockerUri = new Uri("npipe://./pipe/docker_engine");
            }
            else
            {
                // Check for various Docker socket locations on macOS/Linux
                var possibleSockets = new[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rd/docker.sock"), // Rancher Desktop
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".docker/run/docker.sock"), // Docker Desktop
                    "/var/run/docker.sock" // Standard location
                };

                var socketPath = possibleSockets.FirstOrDefault(File.Exists) ?? "/var/run/docker.sock";
                dockerUri = new Uri($"unix://{socketPath}");
                Console.WriteLine($"Using Docker socket: {socketPath}");
            }
            
            Console.WriteLine($"Connecting to Docker at: {dockerUri}");
            var dockerClient = new DockerClientConfiguration(dockerUri).CreateClient();

            Console.WriteLine("Pulling MS SQL Server 2022 image (if not already present)...");
            await dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters { FromImage = "mcr.microsoft.com/mssql/server", Tag = "2022-latest" },
                null,
                new Progress<JSONMessage>());

            if (await StartContainerIfItExists(dockerClient))
            {
                Console.WriteLine("SQL Server container is running.");
                return;
            }

            Console.WriteLine("Creating new SQL Server container...");
            var container = await CreateContainer(dockerClient);

            await dockerClient.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
            Console.WriteLine("SQL Server container started successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to start Docker container. Make sure Docker Desktop is running.");
            Console.WriteLine($"Error details: {ex.Message}");
            throw;
        }
    }

    private static async Task<bool> StartContainerIfItExists(DockerClient dockerClient)
    {
        var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters
        {
            All = true
        });

        var existing = containers.FirstOrDefault(c => c.Names.Any(n => n.TrimStart('/') == "sqlserver"));

        if (existing != null)
        {
            if (existing.State != "running")
            {
                await dockerClient.Containers.StartContainerAsync(existing.ID, new ContainerStartParameters());
            }

            return true;
        }

        return false;
    }

    private static async Task<CreateContainerResponse> CreateContainer(DockerClient dockerClient)
    {
        var container = await dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = "mcr.microsoft.com/mssql/server:2022-latest",
            Name = "sqlserver",
            Env = new List<string>
            {
                "SA_PASSWORD=" + SqlCredentials.Password,
                "ACCEPT_EULA=Y"
            },
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                { "1433", default }
            },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    { "1433/tcp", new List<PortBinding> { new PortBinding { HostPort = "1433" } } }
                }
            }
        });
        return container;
    }
}