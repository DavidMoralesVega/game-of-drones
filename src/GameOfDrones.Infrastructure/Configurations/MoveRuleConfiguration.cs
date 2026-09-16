using GameOfDrones.Domain.Moves;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameOfDrones.Infrastructure.Configurations;

internal sealed class MoveRuleConfiguration : IEntityTypeConfiguration<MoveRule>
{
    public void Configure(EntityTypeBuilder<MoveRule> builder)
    {
        builder.ToTable("MoveRules");

        builder.HasKey(rule => new { rule.WinnerMoveId, rule.LoserMoveId });

        builder.HasOne(rule => rule.Loser)
            .WithMany()
            .HasForeignKey(rule => rule.LoserMoveId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
