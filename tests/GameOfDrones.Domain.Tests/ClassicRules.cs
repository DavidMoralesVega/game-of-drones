using GameOfDrones.Domain.Moves;

namespace GameOfDrones.Domain.Tests;

internal sealed class ClassicRules
{
    public ClassicRules()
    {
        Paper.SetKills([Rock]);
        Rock.SetKills([Scissors]);
        Scissors.SetKills([Paper]);
    }

    public Move Rock { get; } = new("Rock");

    public Move Paper { get; } = new("Paper");

    public Move Scissors { get; } = new("Scissors");
}
