using AdditionApi;
using AdditionApi.Repository;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IAdditionRepository,AdditionRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await DockerStarter.StartDockerContainerAsync();
Database.Setup();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
