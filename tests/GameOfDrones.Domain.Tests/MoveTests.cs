using GameOfDrones.Domain.Moves;

namespace GameOfDrones.Domain.Tests;

public class MoveTests
{
    private readonly ClassicRules _rules = new();

    [Fact]
    public void Beats_follows_the_configured_rules()
    {
        Assert.True(_rules.Paper.Beats(_rules.Rock));
        Assert.True(_rules.Rock.Beats(_rules.Scissors));
        Assert.True(_rules.Scissors.Beats(_rules.Paper));

        Assert.False(_rules.Rock.Beats(_rules.Paper));
        Assert.False(_rules.Rock.Beats(_rules.Rock));
    }

    [Fact]
    public void SetKills_replaces_the_previous_rules()
    {
        var lizard = new Move("Lizard");

        _rules.Rock.SetKills([lizard]);

        Assert.True(_rules.Rock.Beats(lizard));
        Assert.False(_rules.Rock.Beats(_rules.Scissors));
    }

    [Fact]
    public void SetKills_can_target_several_moves()
    {
        var lizard = new Move("Lizard");

        _rules.Rock.SetKills([_rules.Scissors, lizard]);

        Assert.True(_rules.Rock.Beats(_rules.Scissors));
        Assert.True(_rules.Rock.Beats(lizard));
    }

    [Fact]
    public void SetKills_rejects_a_move_killing_itself()
    {
        var exception = Assert.Throws<DomainException>(() => _rules.Rock.SetKills([_rules.Rock]));

        Assert.Contains("cannot kill itself", exception.Message);
    }

    [Fact]
    public void SetKills_rejects_contradicting_an_existing_rule()
    {
        var exception = Assert.Throws<DomainException>(() => _rules.Rock.SetKills([_rules.Paper]));

        Assert.Contains("already kill", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Name_is_required(string? name)
    {
        Assert.Throws<DomainException>(() => new Move(name!));
    }

    [Fact]
    public void Name_is_trimmed_and_length_limited()
    {
        Assert.Equal("Spock", new Move("  Spock ").Name);
        Assert.Throws<DomainException>(() => new Move(new string('x', Move.MaxNameLength + 1)));
    }
}
