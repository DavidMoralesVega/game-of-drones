namespace GameOfDrones.Domain.Players;

public class Player
{
    public const int MaxNameLength = Names.MaxLength;

    private Player()
    {
    }

    public Player(string name)
    {
        Name = Names.Normalize(name, "Player");
        NormalizedName = Names.ToKey(Name);
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string NormalizedName { get; private set; } = null!;

    public bool HasSameNameAs(Player other) => NormalizedName == other.NormalizedName;
}
