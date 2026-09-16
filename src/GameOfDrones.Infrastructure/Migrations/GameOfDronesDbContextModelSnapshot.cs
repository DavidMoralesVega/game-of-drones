using System;
using GameOfDrones.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GameOfDrones.Infrastructure.Migrations
{
    [DbContext(typeof(GameOfDronesDbContext))]
    partial class GameOfDronesDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "10.0.12");

            modelBuilder.Entity("GameOfDrones.Domain.Games.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("FinishedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("Player1Id")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Player2Id")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("TEXT");

                    b.Property<int?>("WinnerId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Player1Id");

                    b.HasIndex("Player2Id");

                    b.HasIndex("WinnerId");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("GameOfDrones.Domain.Games.Round", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("GameId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Number")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Outcome")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("Player1Move")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("TEXT");

                    b.Property<string>("Player2Move")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GameId", "Number")
                        .IsUnique();

                    b.ToTable("Rounds", (string)null);
                });

            modelBuilder.Entity("GameOfDrones.Domain.Moves.Move", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique();

                    b.ToTable("Moves");
                });

            modelBuilder.Entity("GameOfDrones.Domain.Moves.MoveRule", b =>
                {
                    b.Property<int>("WinnerMoveId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoserMoveId")
                        .HasColumnType("INTEGER");

                    b.HasKey("WinnerMoveId", "LoserMoveId");

                    b.HasIndex("LoserMoveId");

                    b.ToTable("MoveRules", (string)null);
                });

            modelBuilder.Entity("GameOfDrones.Domain.Players.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique();

                    b.ToTable("Players");
                });

            modelBuilder.Entity("GameOfDrones.Domain.Games.Game", b =>
                {
                    b.HasOne("GameOfDrones.Domain.Players.Player", "Player1")
                        .WithMany()
                        .HasForeignKey("Player1Id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GameOfDrones.Domain.Players.Player", "Player2")
                        .WithMany()
                        .HasForeignKey("Player2Id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GameOfDrones.Domain.Players.Player", "Winner")
                        .WithMany()
                        .HasForeignKey("WinnerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Player1");

                    b.Navigation("Player2");

                    b.Navigation("Winner");
                });

            modelBuilder.Entity("GameOfDrones.Domain.Games.Round", b =>
                {
                    b.HasOne("GameOfDrones.Domain.Games.Game", null)
                        .WithMany("Rounds")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GameOfDrones.Domain.Moves.MoveRule", b =>
                {
                    b.HasOne("GameOfDrones.Domain.Moves.Move", "Loser")
                        .WithMany()
                        .HasForeignKey("LoserMoveId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GameOfDrones.Domain.Moves.Move", "Winner")
                        .WithMany("Kills")
                        .HasForeignKey("WinnerMoveId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Loser");

                    b.Navigation("Winner");
                });

            modelBuilder.Entity("GameOfDrones.Domain.Games.Game", b =>
                {
                    b.Navigation("Rounds");
                });

            modelBuilder.Entity("GameOfDrones.Domain.Moves.Move", b =>
                {
                    b.Navigation("Kills");
                });
#pragma warning restore 612, 618
        }
    }
}
