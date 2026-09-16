using GameOfDrones.Api.Contracts;
using GameOfDrones.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GameOfDrones.Api.Endpoints;

internal static class PlayerEndpoints
{
    public static void MapPlayers(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/players").WithTags("Players").MapGet("/stats", GetStats);
    }

    private static async Task<Ok<List<PlayerStatsResponse>>> GetStats(GameOfDronesDbContext db, CancellationToken cancellationToken)
    {
        var stats = await db.Players
            .Select(player => new
            {
                player.Name,
                GamesWon = db.Games.Count(game => game.FinishedAt != null && game.WinnerId == player.Id),
                GamesPlayed = db.Games.Count(game => game.FinishedAt != null && (game.Player1Id == player.Id || game.Player2Id == player.Id)),
            })
            .Where(stat => stat.GamesPlayed > 0)
            .OrderByDescending(stat => stat.GamesWon)
            .ThenBy(stat => stat.Name)
            .Select(stat => new PlayerStatsResponse(stat.Name, stat.GamesWon, stat.GamesPlayed))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(stats);
    }
}
