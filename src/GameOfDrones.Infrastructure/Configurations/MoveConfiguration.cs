using GameOfDrones.Domain.Moves;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameOfDrones.Infrastructure.Configurations;

internal sealed class MoveConfiguration : IEntityTypeConfiguration<Move>
{
    public void Configure(EntityTypeBuilder<Move> builder)
    {
        builder.Property(move => move.Name).HasMaxLength(Move.MaxNameLength);
        builder.Property(move => move.NormalizedName).HasMaxLength(Move.MaxNameLength);

        builder.HasIndex(move => move.NormalizedName).IsUnique();

        builder.HasMany(move => move.Kills)
            .WithOne(rule => rule.Winner)
            .HasForeignKey(rule => rule.WinnerMoveId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
