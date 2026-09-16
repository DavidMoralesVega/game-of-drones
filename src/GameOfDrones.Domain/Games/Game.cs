using GameOfDrones.Domain.Moves;
using GameOfDrones.Domain.Players;

namespace GameOfDrones.Domain.Games;

public class Game
{
    public const int WinsToConquer = 3;

    private readonly List<Round> _rounds = [];

    private Game()
    {
    }

    public Game(Player player1, Player player2)
    {
        if (player1.HasSameNameAs(player2))
        {
            throw new DomainException("Players must have different names.");
        }

        Player1 = player1;
        Player2 = player2;
        StartedAt = DateTime.UtcNow;
    }

    public int Id { get; private set; }

    public int Player1Id { get; private set; }

    public Player Player1 { get; private set; } = null!;

    public int Player2Id { get; private set; }

    public Player Player2 { get; private set; } = null!;

    public int? WinnerId { get; private set; }

    public Player? Winner { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime? FinishedAt { get; private set; }

    public IReadOnlyCollection<Round> Rounds => _rounds;

    public bool IsFinished => FinishedAt is not null;

    public int Player1Score => _rounds.Count(round => round.Outcome == RoundOutcome.Player1Wins);

    public int Player2Score => _rounds.Count(round => round.Outcome == RoundOutcome.Player2Wins);

    public Round PlayRound(Move player1Move, Move player2Move)
    {
        if (IsFinished)
        {
            throw new DomainException("The game is already finished.");
        }

        var outcome = Resolve(player1Move, player2Move);
        var round = new Round(_rounds.Count + 1, player1Move.Name, player2Move.Name, outcome);
        _rounds.Add(round);

        if (Player1Score == WinsToConquer)
        {
            Finish(Player1);
        }
        else if (Player2Score == WinsToConquer)
        {
            Finish(Player2);
        }

        return round;
    }

    private static RoundOutcome Resolve(Move player1Move, Move player2Move)
    {
        if (player1Move.Beats(player2Move))
        {
            return RoundOutcome.Player1Wins;
        }

        if (player2Move.Beats(player1Move))
        {
            return RoundOutcome.Player2Wins;
        }

        return RoundOutcome.Draw;
    }

    private void Finish(Player winner)
    {
        Winner = winner;
        FinishedAt = DateTime.UtcNow;
    }
}
