using System.Net;
using System.Net.Http.Json;
using GameOfDrones.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GameOfDrones.Api.Tests;

public sealed class MovesApiTests : IDisposable
{
    private readonly GameOfDronesApp _app = new();
    private readonly HttpClient _client;

    public MovesApiTests()
    {
        _client = _app.CreateClient();
    }

    [Fact]
    public async Task A_new_move_starts_without_rules()
    {
        var response = await _client.PostAsJsonAsync("/api/moves", new CreateMoveRequest(" Lizard "));
        var move = await response.Content.ReadFromJsonAsync<MoveResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Lizard", move!.Name);
        Assert.Empty(move.Kills);
    }

    [Fact]
    public async Task Duplicate_move_names_are_rejected_ignoring_case()
    {
        var response = await _client.PostAsJsonAsync("/api/moves", new CreateMoveRequest("rock"));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("A move named rock already exists.", problem!.Detail);
    }

    [Fact]
    public async Task Rules_can_be_extended_at_runtime_and_unrelated_moves_draw()
    {
        await _client.PostAsJsonAsync("/api/moves", new CreateMoveRequest("Lizard"));
        var moves = await _client.GetMovesByNameAsync();

        var response = await _client.PutAsJsonAsync($"/api/moves/{moves["Rock"].Id}/kills", new UpdateMoveKillsRequest([moves["Scissors"].Id, moves["Lizard"].Id]));
        var rock = await response.Content.ReadFromJsonAsync<MoveResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([moves["Scissors"].Id, moves["Lizard"].Id], rock!.Kills);

        var game = await _client.CreateGameAsync("Alice", "Bob");
        game = await _client.PlayRoundAsync(game.Id, "Rock", "Lizard");
        game = await _client.PlayRoundAsync(game.Id, "Paper", "Lizard");

        Assert.Equal(["Alice", null], game.Rounds.Select(round => round.Winner));
    }

    [Fact]
    public async Task Contradictory_rules_are_rejected()
    {
        var moves = await _client.GetMovesByNameAsync();

        var response = await _client.PutAsJsonAsync($"/api/moves/{moves["Rock"].Id}/kills", new UpdateMoveKillsRequest([moves["Paper"].Id]));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Rock cannot kill Paper because they already kill Rock.", problem!.Detail);
    }

    [Fact]
    public async Task Deleting_a_move_removes_the_rules_that_mention_it()
    {
        await _client.PostAsJsonAsync("/api/moves", new CreateMoveRequest("Lizard"));
        var moves = await _client.GetMovesByNameAsync();
        await _client.PutAsJsonAsync($"/api/moves/{moves["Rock"].Id}/kills", new UpdateMoveKillsRequest([moves["Scissors"].Id, moves["Lizard"].Id]));

        var response = await _client.DeleteAsync($"/api/moves/{moves["Lizard"].Id}");
        var remaining = await _client.GetMovesByNameAsync();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.DoesNotContain("Lizard", remaining.Keys);
        Assert.Equal([moves["Scissors"].Id], remaining["Rock"].Kills);
    }

    [Fact]
    public async Task At_least_two_moves_must_remain()
    {
        var moves = await _client.GetMovesByNameAsync();
        await _client.DeleteAsync($"/api/moves/{moves["Paper"].Id}");

        var response = await _client.DeleteAsync($"/api/moves/{moves["Rock"].Id}");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("At least 2 moves are required to play.", problem!.Detail);
    }

    [Fact]
    public async Task Deleting_an_unknown_move_returns_404()
    {
        var response = await _client.DeleteAsync("/api/moves/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _app.Dispose();
    }
}
