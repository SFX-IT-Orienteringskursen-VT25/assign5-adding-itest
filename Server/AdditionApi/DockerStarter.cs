using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace AdditionApi
{
    public static class DockerStarter
    {
        public static async Task EnsureSqlContainerAsync(IConfiguration configuration, CancellationToken cancellation = default)
        {
            // читаем переменные окружения
            var saPassword = Environment.GetEnvironmentVariable("SA_PASSWORD") ?? "Your_password123!";
            var hostPort = Environment.GetEnvironmentVariable("MSSQL_PORT") ?? "1433";
            var containerName = configuration["Docker:ContainerName"] ?? "sqlserver";
            var image = configuration["Docker:Image"] ?? "mcr.microsoft.com/mssql/server:2022-latest";

            Console.WriteLine($"[DockerStarter] Ensuring SQL Server container '{containerName}'...");

            DockerClient dockerClient;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                dockerClient = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
            else
                dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();

            // 1) Подтягиваем образ
            var imageParts = image.Split(':', 2);
            var imageName = imageParts[0];
            var imageTag = imageParts.Length > 1 ? imageParts[1] : "latest";

            Console.WriteLine($"[DockerStarter] Pulling image {image} ...");
            await dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters { FromImage = imageName, Tag = imageTag },
                null,
                new Progress<JSONMessage>(),
                cancellation);

            // 2) Проверяем контейнер
            var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true }, cancellation);
            var existing = containers.FirstOrDefault(c => c.Names.Any(n => n.TrimStart('/') == containerName));

            if (existing != null)
            {
                if (existing.State != "running")
                {
                    Console.WriteLine($"[DockerStarter] Starting existing container {containerName}...");
                    await dockerClient.Containers.StartContainerAsync(existing.ID, new ContainerStartParameters(), cancellation);
                }
            }
            else
            {
                Console.WriteLine($"[DockerStarter] Creating new container {containerName} ...");

                var createParams = new CreateContainerParameters
                {
                    Image = image,
                    Name = containerName,
                    Env = new List<string>
                    {
                        $"SA_PASSWORD={saPassword}",
                        "ACCEPT_EULA=Y"
                    },
                    ExposedPorts = new Dictionary<string, EmptyStruct>
                    {
                        { "1433/tcp", default }
                    },
                    HostConfig = new HostConfig
                    {
                        PortBindings = new Dictionary<string, IList<PortBinding>>
                        {
                            { "1433/tcp", new List<PortBinding> { new PortBinding { HostPort = hostPort } } }
                        }
                    }
                };

                var created = await dockerClient.Containers.CreateContainerAsync(createParams, cancellation);
                await dockerClient.Containers.StartContainerAsync(created.ID, new ContainerStartParameters(), cancellation);
            }

            // 3) Ждём, пока SQL Server поднимется
            
var dbName = Environment.GetEnvironmentVariable("MSSQL_DB") ?? "MyAppDb";
var connCheck = $"Server=127.0.0.1,{hostPort};Database=master;User Id=sa;Password={saPassword};TrustServerCertificate=True;";

await WaitUntilSqlReadyAsync(connCheck, TimeSpan.FromMinutes(2), cancellation);

        }

        private static async Task WaitUntilSqlReadyAsync(string connectionString, TimeSpan timeout, CancellationToken cancellation)
        {
            var start = DateTime.UtcNow;
            Exception lastEx = null;

            while (DateTime.UtcNow - start < timeout)
            {
                try
                {
                    using var conn = new SqlConnection(connectionString);
                    await conn.OpenAsync(cancellation);
                    await conn.CloseAsync();
                    Console.WriteLine("[DockerStarter] SQL Server is ready.");
                    return;
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    Console.WriteLine($"[DockerStarter] SQL not ready yet: {ex.Message}");
                    await Task.Delay(2000, cancellation);
                }
            }

            throw new TimeoutException($"Timed out waiting for SQL Server. Last error: {lastEx?.Message}", lastEx);
        }
    }
}
