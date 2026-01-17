var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()  
              .AllowAnyMethod()  
              .AllowAnyHeader();  
    });
});

var app = builder.Build();

// Run new Cors
app.UseCors();

// Create new var in memory
int currentNumber = 0;

app.MapGet("/number", () =>
{
    return Results.Ok(currentNumber);
});

app.MapPost("/number", (int number) =>
{{
        // update new number in memory
        currentNumber = number;
    }
    return Results.Ok("Number received");
});


app.Run();