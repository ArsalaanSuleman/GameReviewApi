using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;



namespace GameReviewApi.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public double? Rating { get; set; }

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime? ReleaseDate { get; set; }
        public string? ImageUrl { get; set; }
        public string? Genre { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}