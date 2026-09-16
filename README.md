# Game of Drones

Two players share one computer and battle with Rock, Paper and Scissors. The first player to win 3 rounds becomes the Emperor. Every finished game is stored, so the start screen shows how many battles each player has won, and the moves themselves can be changed at runtime: add new moves and decide which move kills which.

**Live demo:** https://game-of-drones-55210615749.us-east4.run.app

## Stack

| Layer | Technology |
| --- | --- |
| Frontend | Angular 22 (standalone components, signals, zoneless), Tailwind CSS 4 |
| Backend | ASP.NET Core 10 Minimal APIs, OpenAPI |
| Data | Entity Framework Core 10 with SQLite, code-first migrations applied on startup |
| Tests | xUnit for the domain rules and the HTTP API (in-memory SQLite), Vitest for the Angular store |
| Hosting | Single Docker image (API + SPA) on Google Cloud Run |

## Run it from Visual Studio

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) with Visual Studio 2026 or Visual Studio 2022 17.14+, and [Node.js](https://nodejs.org/) 22.22+ or 24.15+ (Angular 22 requirement).

1. Open `GameOfDrones.sln`.
2. Press **F5** on `GameOfDrones.Api`.

Nothing else is needed. On the first build the API project runs `npm ci` for the Angular app, on startup it creates the SQLite database with the classic rules, and the SPA proxy launches `ng serve` and opens the browser on the game. Requests to `/api` are proxied to the API, so there is no CORS configuration.

## Run it from the command line

```bash
dotnet run --project src/GameOfDrones.Api
```

Then open http://localhost:5073. The API documentation (Scalar UI) is available at http://localhost:5073/scalar while running in Development.

Run the backend tests (domain unit tests plus API integration tests against an in-memory SQLite database):

```bash
dotnet test
```

Run the Angular tests:

```bash
cd src/GameOfDrones.Web && npm test
```

## How the game works

1. Enter both player names and press **Start**.
2. Player 1 picks a move. The screen then asks to pass the device to Player 2 and only shows the move selector again once Player 2 confirms, so the first choice is never on screen for the second player.
3. The API resolves the round and the score board on the right lists every round and its winner.
4. When a player reaches 3 wins the Emperor screen appears. **Play Again** returns to the start screen.
5. The start screen shows the *Hall of Emperors*: games won and played per player, persisted in the database. Names are matched case-insensitively, so *Ana* and *ANA* are the same player.

### Changing the moves at runtime

Open **Rules** from the start screen. The editor is only reachable between games, never during one. Moves can be added or removed and, for every move, you tick the moves it kills. Changes are saved immediately and apply to the next game. Rules are stored in the database as a directed relation `winner move -> loser move`, which supports both the classic triangle and larger sets such as:

```
Paper kills Rock, Rock kills Scissors, Scissors kills String, String kills Dog, Dog kills Paper
```

When there is no rule between the two moves played (or both players pick the same move) nobody wins the round and the game continues. The domain rejects a move killing itself and contradictory pairs (A kills B while B already kills A), and at least two moves must remain.

## API

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/games` | Start a game with two player names |
| `GET` | `/api/games/{id}` | Current state of a game (score, rounds, winner) |
| `POST` | `/api/games/{id}/rounds` | Play a round with both moves; returns the updated game |
| `GET` | `/api/players/stats` | Games won and played per player |
| `GET` | `/health` | Liveness check including database connectivity |
| `GET` | `/api/moves` | Moves and the moves each one kills |
| `POST` | `/api/moves` | Add a move |
| `DELETE` | `/api/moves/{id}` | Remove a move and its rules |
| `PUT` | `/api/moves/{id}/kills` | Replace the set of moves a move kills |

Rule violations return `400` as RFC 9457 Problem Details with a human-readable `detail` that the UI shows as-is.

## Architecture

```
src/GameOfDrones.Domain          Entities with behaviour (Game, Round, Move, MoveRule, Player). No framework references.
src/GameOfDrones.Infrastructure  EF Core DbContext, entity configurations, migrations, startup seeding.
src/GameOfDrones.Api             Minimal API endpoints grouped by feature, request/response contracts, error handling, SPA hosting.
src/GameOfDrones.Web             Angular app: core (API client, signal store, Vitest specs), features (start, game, winner, rules), shared (score board).
tests/GameOfDrones.Domain.Tests  xUnit tests for the round resolution, win condition and rule validation.
tests/GameOfDrones.Api.Tests     xUnit integration tests that exercise every endpoint over HTTP with an in-memory SQLite database.
```

Decisions worth knowing:

- **The domain owns the rules.** `Game.PlayRound` resolves the round and detects the winner; `Move.SetKills` validates rule changes. This logic is framework-free and covered by unit tests. Invariants that need the whole collection, such as unique names or keeping at least two moves, are enforced at the API layer where the data lives, backed by unique indexes.
- **Case-insensitive names without database tricks.** Players and moves store a normalized key next to the display name (the same approach ASP.NET Identity uses), so uniqueness and lookups behave the same on any database provider.
- **The API is the source of truth.** The Angular store only tracks whose turn it is and the first player's hidden move; scores and outcomes always come from the server response.
- **Rounds store move names as snapshots**, so rules can change after a game without rewriting its history.
- **No repositories or CQRS layers.** For this scope the EF Core `DbContext` is the unit of work; adding more abstraction would only add indirection.
- **Configuration over hard-coding.** The connection string lives in `appsettings.json` and can be overridden with the `ConnectionStrings__GameOfDrones` environment variable; the SPA calls the API through a relative base URL, so it works in development (proxy) and production (same origin) without changes.

## Deployment

The `Dockerfile` builds the Angular app, publishes the API and produces one image that serves both. Deploy it to Cloud Run straight from the source tree:

```bash
gcloud run deploy game-of-drones --source . --region us-east4 --allow-unauthenticated --min-instances 1 --max-instances 1 --cpu-boost --memory 512Mi
```

The demo keeps a single warm instance so the SQLite file on the container survives between games. For a production deployment, point `ConnectionStrings__GameOfDrones` at a managed SQL database; EF Core makes that a provider swap plus regenerated migrations.

---

David Morales Vega
