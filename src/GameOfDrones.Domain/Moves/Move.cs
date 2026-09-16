namespace GameOfDrones.Domain.Moves;

public class Move
{
    public const int MaxNameLength = Names.MaxLength;

    private readonly List<MoveRule> _kills = [];

    private Move()
    {
    }

    public Move(string name)
    {
        Name = Names.Normalize(name, "Move");
        NormalizedName = Names.ToKey(Name);
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string NormalizedName { get; private set; } = null!;

    public IReadOnlyCollection<MoveRule> Kills => _kills;

    public bool Beats(Move other) => _kills.Any(rule => ReferenceEquals(rule.Loser, other));

    public void SetKills(IReadOnlyCollection<Move> losers)
    {
        if (losers.Contains(this))
        {
            throw new DomainException($"{Name} cannot kill itself.");
        }

        var contradictions = losers.Where(loser => loser.Beats(this)).Select(loser => loser.Name).ToList();
        if (contradictions.Count > 0)
        {
            throw new DomainException($"{Name} cannot kill {string.Join(", ", contradictions)} because they already kill {Name}.");
        }

        _kills.RemoveAll(rule => !losers.Contains(rule.Loser));
        _kills.AddRange(losers.Distinct().Where(loser => !Beats(loser)).Select(loser => new MoveRule(this, loser)));
    }
}
