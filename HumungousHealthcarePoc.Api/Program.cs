var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDaprClient();
builder.Services.AddValidatorsFromAssembly(Assembly.Load("HumungousHealthcarePoc.Api"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (true || app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

var health = app.MapGroup("/healthdata").WithOpenApi();

health.MapPost("", async (DaprClient client, HealthDataItem item, HealthDataItemValidator validator) =>
{
    var validation = validator.Validate(item);

    if (!validation.IsValid)
    {
        return Results.ValidationProblem(validation.ToDictionary());
    }

    var id = Guid.NewGuid().ToString();
    var keyedItem = item with { Id = id };
    await client.SaveStateAsync("statestore", id, keyedItem);

    return Results.Created($"/healthdata/{id}", keyedItem);
})
.WithName("PostHealthData");

health.MapGet("/{id}", async (DaprClient client, Guid id) =>
{
    var item = await client.GetStateAsync<HealthDataItem>("statestore", id.ToString());

    return item is null ? Results.NotFound(id) : Results.Ok(item);
})
.WithName("GetHealthData");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
