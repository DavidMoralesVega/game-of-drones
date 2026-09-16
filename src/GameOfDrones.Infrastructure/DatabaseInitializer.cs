using GameOfDrones.Domain.Moves;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GameOfDrones.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GameOfDronesDbContext>();

        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Moves.AnyAsync(cancellationToken))
        {
            return;
        }

        var rock = new Move("Rock");
        var paper = new Move("Paper");
        var scissors = new Move("Scissors");

        db.Moves.AddRange(rock, paper, scissors);
        await db.SaveChangesAsync(cancellationToken);

        paper.SetKills([rock]);
        rock.SetKills([scissors]);
        scissors.SetKills([paper]);
        await db.SaveChangesAsync(cancellationToken);
    }
}
