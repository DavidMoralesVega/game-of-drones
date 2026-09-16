using GameOfDrones.Domain.Games;
using GameOfDrones.Domain.Moves;
using GameOfDrones.Domain.Players;

namespace GameOfDrones.Api.Contracts;

internal static class ResponseMapping
{
    public static GameResponse ToResponse(this Game game) => new(
        game.Id,
        game.Player1.ToResponse(),
        game.Player2.ToResponse(),
        game.Player1Score,
        game.Player2Score,
        Game.WinsToConquer,
        game.IsFinished,
        game.Winner?.Name,
        game.Rounds.Select(round => round.ToResponse(game)).ToList());

    public static MoveResponse ToResponse(this Move move) =>
        new(move.Id, move.Name, move.Kills.Select(rule => rule.LoserMoveId).ToList());

    private static PlayerResponse ToResponse(this Player player) => new(player.Id, player.Name);

    private static RoundResponse ToResponse(this Round round, Game game) => new(
        round.Number,
        round.Player1Move,
        round.Player2Move,
        round.Outcome switch
        {
            RoundOutcome.Player1Wins => game.Player1.Name,
            RoundOutcome.Player2Wins => game.Player2.Name,
            _ => null,
        });
}
