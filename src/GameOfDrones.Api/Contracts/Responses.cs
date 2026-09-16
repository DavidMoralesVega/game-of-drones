namespace GameOfDrones.Api.Contracts;

public sealed record PlayerResponse(int Id, string Name);

public sealed record RoundResponse(int Number, string Player1Move, string Player2Move, string? Winner);

public sealed record GameResponse(
    int Id,
    PlayerResponse Player1,
    PlayerResponse Player2,
    int Player1Score,
    int Player2Score,
    int WinsToConquer,
    bool IsFinished,
    string? Winner,
    IReadOnlyList<RoundResponse> Rounds);

public sealed record MoveResponse(int Id, string Name, IReadOnlyList<int> Kills);

public sealed record PlayerStatsResponse(string Name, int GamesWon, int GamesPlayed);
