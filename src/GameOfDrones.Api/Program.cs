using GameOfDrones.Api.Endpoints;
using GameOfDrones.Api.ErrorHandling;
using GameOfDrones.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("GameOfDrones")
    ?? throw new InvalidOperationException("Connection string 'GameOfDrones' is not configured.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddOpenApi();

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGames();
app.MapMoves();
app.MapPlayers();

app.MapFallbackToFile("index.html");

app.Run();
