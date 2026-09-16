using GameOfDrones.Domain.Games;
using GameOfDrones.Domain.Moves;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameOfDrones.Infrastructure.Configurations;

internal sealed class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.ToTable("Rounds");

        builder.Property(round => round.Player1Move).HasMaxLength(Move.MaxNameLength);
        builder.Property(round => round.Player2Move).HasMaxLength(Move.MaxNameLength);
        builder.Property(round => round.Outcome).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(round => new { round.GameId, round.Number }).IsUnique();
    }
}
