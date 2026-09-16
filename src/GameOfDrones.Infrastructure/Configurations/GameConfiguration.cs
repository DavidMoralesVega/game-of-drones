using GameOfDrones.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameOfDrones.Infrastructure.Configurations;

internal sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasOne(game => game.Player1)
            .WithMany()
            .HasForeignKey(game => game.Player1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(game => game.Player2)
            .WithMany()
            .HasForeignKey(game => game.Player2Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(game => game.Winner)
            .WithMany()
            .HasForeignKey(game => game.WinnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(game => game.Rounds)
            .WithOne()
            .HasForeignKey(round => round.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
