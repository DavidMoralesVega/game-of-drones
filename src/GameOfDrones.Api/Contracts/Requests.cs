namespace GameOfDrones.Api.Contracts;

public sealed record CreateGameRequest(string Player1Name, string Player2Name);

public sealed record PlayRoundRequest(int Player1MoveId, int Player2MoveId);

public sealed record CreateMoveRequest(string Name);

public sealed record UpdateMoveKillsRequest(IReadOnlyList<int>? Kills);
