using System.Net;
using System.Net.Http.Json;
using GameOfDrones.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GameOfDrones.Api.Tests;

public sealed class GamesApiTests : IDisposable
{
    private readonly GameOfDronesApp _app = new();
    private readonly HttpClient _client;

    public GamesApiTests()
    {
        _client = _app.CreateClient();
    }

    [Fact]
    public async Task Database_is_seeded_with_the_classic_rules()
    {
        var moves = await _client.GetMovesByNameAsync();

        Assert.Equal(["Paper", "Rock", "Scissors"], moves.Keys.Order());
        Assert.Equal([moves["Rock"].Id], moves["Paper"].Kills);
        Assert.Equal([moves["Scissors"].Id], moves["Rock"].Kills);
        Assert.Equal([moves["Paper"].Id], moves["Scissors"].Kills);
    }

    [Fact]
    public async Task Creating_a_game_returns_201_with_its_location()
    {
        var response = await _client.PostAsJsonAsync("/api/games", new CreateGameRequest("Alice", "Bob"));
        var game = await response.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/games/{game!.Id}", response.Headers.Location?.PathAndQuery);
        Assert.Equal("Alice", game.Player1.Name);
        Assert.Equal("Bob", game.Player2.Name);
        Assert.Equal(0, game.Player1Score);
        Assert.Equal(0, game.Player2Score);
        Assert.False(game.IsFinished);
    }

    [Fact]
    public async Task Players_with_the_same_name_are_rejected_with_problem_details()
    {
        var response = await _client.PostAsJsonAsync("/api/games", new CreateGameRequest("Alice", "alice"));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Players must have different names.", problem!.Detail);
    }

    [Fact]
    public async Task Player_names_are_matched_ignoring_case_and_whitespace()
    {
        var first = await _client.CreateGameAsync("  Ana ", "Luis");
        var second = await _client.CreateGameAsync("ANA", "luis");

        Assert.Equal(first.Player1.Id, second.Player1.Id);
        Assert.Equal(first.Player2.Id, second.Player2.Id);
        Assert.Equal("Ana", second.Player1.Name);
    }

    [Fact]
    public async Task Rounds_are_resolved_by_the_rules_and_draws_do_not_score()
    {
        var game = await _client.CreateGameAsync("Alice", "Bob");

        game = await _client.PlayRoundAsync(game.Id, "Paper", "Rock");
        game = await _client.PlayRoundAsync(game.Id, "Rock", "Rock");
        game = await _client.PlayRoundAsync(game.Id, "Rock", "Paper");

        Assert.Equal(1, game.Player1Score);
        Assert.Equal(1, game.Player2Score);
        Assert.Equal(["Alice", null, "Bob"], game.Rounds.Select(round => round.Winner));
        Assert.Equal([1, 2, 3], game.Rounds.Select(round => round.Number));
    }

    [Fact]
    public async Task First_player_to_three_wins_finishes_the_game_and_no_more_rounds_are_accepted()
    {
        var game = await _client.CreateGameAsync("Alice", "Bob");
        for (var i = 0; i < 3; i++)
        {
            game = await _client.PlayRoundAsync(game.Id, "Scissors", "Paper");
        }

        Assert.True(game.IsFinished);
        Assert.Equal("Alice", game.Winner);
        Assert.Equal(3, game.WinsToConquer);

        var moves = await _client.GetMovesByNameAsync();
        var extra = await _client.PostAsJsonAsync($"/api/games/{game.Id}/rounds", new PlayRoundRequest(moves["Rock"].Id, moves["Rock"].Id));
        var problem = await extra.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, extra.StatusCode);
        Assert.Equal("The game is already finished.", problem!.Detail);
    }

    [Fact]
    public async Task Finished_game_can_be_read_back_with_its_winner()
    {
        var game = await _client.CreateGameAsync("Alice", "Bob");
        for (var i = 0; i < 3; i++)
        {
            await _client.PlayRoundAsync(game.Id, "Rock", "Scissors");
        }

        var stored = await _client.GetFromJsonAsync<GameResponse>($"/api/games/{game.Id}");

        Assert.True(stored!.IsFinished);
        Assert.Equal("Alice", stored.Winner);
        Assert.Equal(3, stored.Rounds.Count);
    }

    [Fact]
    public async Task Unknown_game_returns_404()
    {
        var response = await _client.GetAsync("/api/games/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_move_is_rejected()
    {
        var game = await _client.CreateGameAsync("Alice", "Bob");

        var response = await _client.PostAsJsonAsync($"/api/games/{game.Id}/rounds", new PlayRoundRequest(999, 1));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Move 999 does not exist.", problem!.Detail);
    }

    [Fact]
    public async Task Player_stats_count_finished_games_only()
    {
        var finished = await _client.CreateGameAsync("Alice", "Bob");
        for (var i = 0; i < 3; i++)
        {
            await _client.PlayRoundAsync(finished.Id, "Paper", "Rock");
        }

        var abandoned = await _client.CreateGameAsync("Alice", "Carol");
        await _client.PlayRoundAsync(abandoned.Id, "Rock", "Scissors");

        var stats = await _client.GetFromJsonAsync<List<PlayerStatsResponse>>("/api/players/stats");

        Assert.Equal([new PlayerStatsResponse("Alice", 1, 1), new PlayerStatsResponse("Bob", 0, 1)], stats);
    }

    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    public void Dispose()
    {
        _client.Dispose();
        _app.Dispose();
    }
}
