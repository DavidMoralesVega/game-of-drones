using GameOfDrones.Domain.Games;
using GameOfDrones.Domain.Moves;
using GameOfDrones.Domain.Players;

namespace GameOfDrones.Domain.Tests;

public class GameTests
{
    private readonly ClassicRules _rules = new();
    private readonly Player _alice = new("Alice");
    private readonly Player _bob = new("Bob");

    [Theory]
    [InlineData("alice")]
    [InlineData("ALICE")]
    [InlineData("  Alice ")]
    public void Players_must_have_different_names_ignoring_case(string sameName)
    {
        var exception = Assert.Throws<DomainException>(() => new Game(_alice, new Player(sameName)));

        Assert.Contains("different names", exception.Message);
    }

    [Fact]
    public void Accented_letters_are_compared_case_insensitively_too()
    {
        Assert.Equal(new Player("José").NormalizedName, new Player("JOSÉ").NormalizedName);
        Assert.NotEqual(new Player("Alice").NormalizedName, new Player("Álice").NormalizedName);
    }

    [Fact]
    public void Round_winner_is_the_player_whose_move_kills_the_other()
    {
        var game = new Game(_alice, _bob);

        var round = game.PlayRound(_rules.Paper, _rules.Rock);

        Assert.Equal(1, round.Number);
        Assert.Equal(RoundOutcome.Player1Wins, round.Outcome);
        Assert.Equal(1, game.Player1Score);
        Assert.Equal(0, game.Player2Score);
    }

    [Fact]
    public void Same_move_is_a_draw_and_the_game_continues()
    {
        var game = new Game(_alice, _bob);

        var round = game.PlayRound(_rules.Rock, _rules.Rock);

        Assert.Equal(RoundOutcome.Draw, round.Outcome);
        Assert.Equal(0, game.Player1Score);
        Assert.Equal(0, game.Player2Score);
        Assert.False(game.IsFinished);
    }

    [Fact]
    public void Unrelated_moves_are_a_draw()
    {
        var dog = new Move("Dog");
        var game = new Game(_alice, _bob);

        var round = game.PlayRound(_rules.Rock, dog);

        Assert.Equal(RoundOutcome.Draw, round.Outcome);
    }

    [Fact]
    public void Round_numbers_keep_counting_through_draws()
    {
        var game = new Game(_alice, _bob);

        game.PlayRound(_rules.Rock, _rules.Rock);
        var second = game.PlayRound(_rules.Rock, _rules.Scissors);

        Assert.Equal(2, second.Number);
        Assert.Equal(2, game.Rounds.Count);
    }

    [Fact]
    public void Rounds_record_the_move_names_played()
    {
        var game = new Game(_alice, _bob);

        var round = game.PlayRound(_rules.Scissors, _rules.Paper);

        Assert.Equal("Scissors", round.Player1Move);
        Assert.Equal("Paper", round.Player2Move);
    }

    [Fact]
    public void First_player_to_reach_the_required_wins_conquers()
    {
        var game = new Game(_alice, _bob);

        game.PlayRound(_rules.Paper, _rules.Rock);
        game.PlayRound(_rules.Rock, _rules.Paper);
        game.PlayRound(_rules.Paper, _rules.Rock);
        game.PlayRound(_rules.Rock, _rules.Rock);
        Assert.False(game.IsFinished);

        game.PlayRound(_rules.Paper, _rules.Rock);

        Assert.True(game.IsFinished);
        Assert.Same(_alice, game.Winner);
        Assert.Equal(Game.WinsToConquer, game.Player1Score);
        Assert.Equal(1, game.Player2Score);
    }

    [Fact]
    public void Second_player_can_conquer_too()
    {
        var game = new Game(_alice, _bob);

        for (var i = 0; i < Game.WinsToConquer; i++)
        {
            game.PlayRound(_rules.Rock, _rules.Paper);
        }

        Assert.True(game.IsFinished);
        Assert.Same(_bob, game.Winner);
    }

    [Fact]
    public void Finished_game_rejects_more_rounds()
    {
        var game = new Game(_alice, _bob);
        for (var i = 0; i < Game.WinsToConquer; i++)
        {
            game.PlayRound(_rules.Paper, _rules.Rock);
        }

        var exception = Assert.Throws<DomainException>(() => game.PlayRound(_rules.Paper, _rules.Rock));

        Assert.Contains("already finished", exception.Message);
    }
}
