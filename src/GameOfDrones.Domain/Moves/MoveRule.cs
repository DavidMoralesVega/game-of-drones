namespace GameOfDrones.Domain.Moves;

public class MoveRule
{
    private MoveRule()
    {
    }

    internal MoveRule(Move winner, Move loser)
    {
        Winner = winner;
        Loser = loser;
    }

    public int WinnerMoveId { get; private set; }

    public Move Winner { get; private set; } = null!;

    public int LoserMoveId { get; private set; }

    public Move Loser { get; private set; } = null!;
}
