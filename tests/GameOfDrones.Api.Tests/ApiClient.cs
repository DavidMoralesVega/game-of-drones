using System.Net.Http.Json;
using GameOfDrones.Api.Contracts;

namespace GameOfDrones.Api.Tests;

internal static class ApiClient
{
    public static async Task<IReadOnlyDictionary<string, MoveResponse>> GetMovesByNameAsync(this HttpClient client)
    {
        var moves = await client.GetFromJsonAsync<List<MoveResponse>>("/api/moves");

        return moves!.ToDictionary(move => move.Name);
    }

    public static async Task<GameResponse> CreateGameAsync(this HttpClient client, string player1, string player2)
    {
        var response = await client.PostAsJsonAsync("/api/games", new CreateGameRequest(player1, player2));
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<GameResponse>())!;
    }

    public static async Task<GameResponse> PlayRoundAsync(this HttpClient client, int gameId, string player1Move, string player2Move)
    {
        var moves = await client.GetMovesByNameAsync();
        var request = new PlayRoundRequest(moves[player1Move].Id, moves[player2Move].Id);
        var response = await client.PostAsJsonAsync($"/api/games/{gameId}/rounds", request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<GameResponse>())!;
    }
}
