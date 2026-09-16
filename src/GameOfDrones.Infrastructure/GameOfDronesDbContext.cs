using GameOfDrones.Domain.Games;
using GameOfDrones.Domain.Moves;
using GameOfDrones.Domain.Players;
using Microsoft.EntityFrameworkCore;

namespace GameOfDrones.Infrastructure;

public class GameOfDronesDbContext(DbContextOptions<GameOfDronesDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();

    public DbSet<Move> Moves => Set<Move>();

    public DbSet<Game> Games => Set<Game>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameOfDronesDbContext).Assembly);
    }
}
