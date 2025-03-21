namespace GameReviewApi.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int UserId { get; set; }
        public string? Comment { get; set; }
        public double Rating { get; set; }
        public Game? Game { get; set; }
        public User? User { get; set; }

    }
}
