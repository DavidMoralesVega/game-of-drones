using GameOfDrones.Domain.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameOfDrones.Infrastructure.Configurations;

internal sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.Property(player => player.Name).HasMaxLength(Player.MaxNameLength);
        builder.Property(player => player.NormalizedName).HasMaxLength(Player.MaxNameLength);

        builder.HasIndex(player => player.NormalizedName).IsUnique();
    }
}
