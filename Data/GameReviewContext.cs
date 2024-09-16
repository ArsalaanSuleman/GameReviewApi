
using Microsoft.EntityFrameworkCore;
using GameReviewApi.Models;


namespace GameReviewApi.Data
{
public class GameReviewContext : DbContext
{
    public GameReviewContext(DbContextOptions<GameReviewContext> options)
        : base(options){ }

    public DbSet<Game> Games { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Game)
            .WithMany(g => g.Reviews)
            .HasForeignKey(r => r.GameId);

        base.OnModelCreating(modelBuilder);
    }
}

}