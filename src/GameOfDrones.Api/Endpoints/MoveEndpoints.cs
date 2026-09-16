using GameOfDrones.Api.Contracts;
using GameOfDrones.Domain;
using GameOfDrones.Domain.Moves;
using GameOfDrones.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GameOfDrones.Api.Endpoints;

internal static class MoveEndpoints
{
    private const int MinimumMoves = 2;

    public static void MapMoves(this IEndpointRouteBuilder app)
    {
        var moves = app.MapGroup("/api/moves").WithTags("Moves");

        moves.MapGet("/", ListMoves);
        moves.MapPost("/", CreateMove);
        moves.MapDelete("/{id:int}", DeleteMove);
        moves.MapPut("/{id:int}/kills", UpdateKills);
    }

    private static async Task<Ok<List<MoveResponse>>> ListMoves(GameOfDronesDbContext db, CancellationToken cancellationToken)
    {
        var moves = await db.Moves
            .Include(move => move.Kills)
            .OrderBy(move => move.Id)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(moves.Select(move => move.ToResponse()).ToList());
    }

    private static async Task<Created<MoveResponse>> CreateMove(
        CreateMoveRequest request,
        GameOfDronesDbContext db,
        CancellationToken cancellationToken)
    {
        var move = new Move(request.Name);

        if (await db.Moves.AnyAsync(existing => existing.NormalizedName == move.NormalizedName, cancellationToken))
        {
            throw new DomainException($"A move named {move.Name} already exists.");
        }

        db.Moves.Add(move);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/moves/{move.Id}", move.ToResponse());
    }

    private static async Task<Results<NoContent, NotFound>> DeleteMove(
        int id,
        GameOfDronesDbContext db,
        CancellationToken cancellationToken)
    {
        var move = await db.Moves.FindAsync([id], cancellationToken);
        if (move is null)
        {
            return TypedResults.NotFound();
        }

        if (await db.Moves.CountAsync(cancellationToken) <= MinimumMoves)
        {
            throw new DomainException($"At least {MinimumMoves} moves are required to play.");
        }

        db.Moves.Remove(move);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<MoveResponse>, NotFound>> UpdateKills(
        int id,
        UpdateMoveKillsRequest request,
        GameOfDronesDbContext db,
        CancellationToken cancellationToken)
    {
        var moves = await db.Moves
            .Include(move => move.Kills)
            .ThenInclude(rule => rule.Loser)
            .ToListAsync(cancellationToken);

        var move = moves.FirstOrDefault(candidate => candidate.Id == id);
        if (move is null)
        {
            return TypedResults.NotFound();
        }

        var losers = (request.Kills ?? [])
            .Select(loserId => moves.FirstOrDefault(candidate => candidate.Id == loserId)
                ?? throw new DomainException($"Move {loserId} does not exist."))
            .ToList();

        move.SetKills(losers);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(move.ToResponse());
    }
}
