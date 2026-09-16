using GameOfDrones.Api.Contracts;
using GameOfDrones.Domain;
using GameOfDrones.Domain.Games;
using GameOfDrones.Domain.Moves;
using GameOfDrones.Domain.Players;
using GameOfDrones.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GameOfDrones.Api.Endpoints;

internal static class GameEndpoints
{
    private const string GetGameRouteName = "GetGame";

    public static void MapGames(this IEndpointRouteBuilder app)
    {
        var games = app.MapGroup("/api/games").WithTags("Games");

        games.MapPost("/", CreateGame);
        games.MapGet("/{id:int}", GetGame).WithName(GetGameRouteName);
        games.MapPost("/{id:int}/rounds", PlayRound);
    }

    private static async Task<CreatedAtRoute<GameResponse>> CreateGame(
        CreateGameRequest request,
        GameOfDronesDbContext db,
        CancellationToken cancellationToken)
    {
        var player1 = await FindOrCreatePlayer(request.Player1Name, db, cancellationToken);
        var player2 = await FindOrCreatePlayer(request.Player2Name, db, cancellationToken);

        var game = new Game(player1, player2);
        db.Games.Add(game);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.CreatedAtRoute(game.ToResponse(), GetGameRouteName, new { id = game.Id });
    }

    private static async Task<Results<Ok<GameResponse>, NotFound>> GetGame(
        int id,
        GameOfDronesDbContext db,
        CancellationToken cancellationToken)
    {
        var game = await LoadGame(id, db, cancellationToken);

        return game is null ? TypedResults.NotFound() : TypedResults.Ok(game.ToResponse());
    }

    private static async Task<Results<Ok<GameResponse>, NotFound>> PlayRound(
        int id,
        PlayRoundRequest request,
        GameOfDronesDbContext db,
        CancellationToken cancellationToken)
    {
        var game = await LoadGame(id, db, cancellationToken);
        if (game is null)
        {
            return TypedResults.NotFound();
        }

        var moves = await db.Moves
            .Include(move => move.Kills)
            .ThenInclude(rule => rule.Loser)
            .ToListAsync(cancellationToken);

        game.PlayRound(FindMove(moves, request.Player1MoveId), FindMove(moves, request.Player2MoveId));
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(game.ToResponse());
    }

    private static Task<Game?> LoadGame(int id, GameOfDronesDbContext db, CancellationToken cancellationToken) =>
        db.Games
            .Include(game => game.Player1)
            .Include(game => game.Player2)
            .Include(game => game.Winner)
            .Include(game => game.Rounds.OrderBy(round => round.Number))
            .FirstOrDefaultAsync(game => game.Id == id, cancellationToken);

    private static async Task<Player> FindOrCreatePlayer(string name, GameOfDronesDbContext db, CancellationToken cancellationToken)
    {
        var player = new Player(name);
        var existing = await db.Players.FirstOrDefaultAsync(candidate => candidate.NormalizedName == player.NormalizedName, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        db.Players.Add(player);
        return player;
    }

    private static Move FindMove(IReadOnlyCollection<Move> moves, int moveId) =>
        moves.FirstOrDefault(move => move.Id == moveId)
            ?? throw new DomainException($"Move {moveId} does not exist.");
}
