namespace GameOfDrones.Domain.Games;

public class Round
{
    private Round()
    {
    }

    internal Round(int number, string player1Move, string player2Move, RoundOutcome outcome)
    {
        Number = number;
        Player1Move = player1Move;
        Player2Move = player2Move;
        Outcome = outcome;
    }

    public int Id { get; private set; }

    public int GameId { get; private set; }

    public int Number { get; private set; }

    public string Player1Move { get; private set; } = null!;

    public string Player2Move { get; private set; } = null!;

    public RoundOutcome Outcome { get; private set; }
}
