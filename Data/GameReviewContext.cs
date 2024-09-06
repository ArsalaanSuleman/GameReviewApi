
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
}

}