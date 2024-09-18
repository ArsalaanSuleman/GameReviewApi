namespace GameReviewApi.Models
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string? Comment { get; set; }
        public double Rating { get; set; }
        public string? Username { get; set; } // For returning the username
    }
}
