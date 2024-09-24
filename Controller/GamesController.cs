using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameReviewApi.Data;
using GameReviewApi.Models;
using System.IO;
using Microsoft.AspNetCore.Authorization;


namespace GameReviewApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly GameReviewContext _context;

        public GamesController(GameReviewContext context)
        {
            _context = context;
        }

        [AllowAnonymous] // Add this attribute to allow anonymous access to this endpoint
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Game>>> GetGames()
        {
            var games = await _context.Games.ToListAsync();
            var gameDtos = games.Select(game => new
            {
                game.Id,
                game.Title,
                game.Genre,
                game.Description,
                game.ReleaseDate,
                ImageUrl = $"/images/{Path.GetFileName(game.ImageUrl)}" // Return the relative URL
            });

            return Ok(gameDtos);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetGame(int id)
        {
            var game = await _context.Games
                .Where(g => g.Id == id)
                .Select(g => new
                {
                    g.Id,
                    g.Title,
                    g.Description,
                    g.Genre,
                    g.ReleaseDate,
                    g.ImageUrl,
                    Reviews = g.Reviews.Select(r => new
                    {
                        r.Id,
                        r.Comment,
                        r.Rating,
                        r.UserId
                    })
                })
                .FirstOrDefaultAsync();

            if (game == null)
            {
                return NotFound();
            }

            return Ok(game);
        }

        [HttpPost]
        public async Task<IActionResult> PostGame([FromForm] Game newGame, [FromForm] IFormFile image)
        {
            if (newGame == null)
            {
                return BadRequest("Game data is missing.");
            }

            if (image != null && image.Length > 0)
            {
                // Define the folder to save the image
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                var fileName = Path.GetFileName(image.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Check if the directory exists, if not, create it
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Copy the image to the specified path
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Save the relative image URL to the game entity
                newGame.ImageUrl = $"/images/{fileName}";
            }

            // If release date is provided, set the proper DateTimeKind
            if (newGame.ReleaseDate.HasValue)
            {
                newGame.ReleaseDate = DateTime.SpecifyKind(newGame.ReleaseDate.Value, DateTimeKind.Utc);
            }

            // Add the new game to the context and save changes
            _context.Games.Add(newGame);
            await _context.SaveChangesAsync();

            // Return the created game
            return CreatedAtAction(nameof(GetGame), new { id = newGame.Id }, newGame);
        }


        [HttpGet("search")]
        public async Task<IActionResult> SearchGames([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Query cannot be empty");
            }

            var matchingGames = await _context.Games
                .Where(g => EF.Functions.Like(g.Title.ToLower(), $"%{query.ToLower()}%") ||
                            EF.Functions.Like(g.Description.ToLower(), $"%{query.ToLower()}%"))
                .Select(game => new
                {
                    game.Id,
                    game.Title,
                    game.Genre,
                    game.Description,
                    game.ReleaseDate,
                    ImageUrl = $"/images/{Path.GetFileName(game.ImageUrl)}" // Ensure consistent image URL
                })
                .ToListAsync();

            if (matchingGames.Count == 0)
            {
                return NotFound("No games found matching the search query");
            }

            return Ok(matchingGames);
        }

        [HttpGet("{gameId}/average-rating")]
        public async Task<IActionResult> GetAverageRating(int gameId)
        {
            // Fetch all reviews for the game
            var reviews = await _context.Reviews
                .Where(r => r.GameId == gameId)
                .ToListAsync();

            // If no reviews found, return 0 for the average rating
            double averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            return Ok(new { averageRating });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutGame(int id, [FromForm] Game updatedGame, [FromForm] IFormFile image)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            game.Title = updatedGame.Title;
            game.Genre = updatedGame.Genre;
            game.Description = updatedGame.Description;

            if (updatedGame.ReleaseDate.HasValue)
            {
                game.ReleaseDate = DateTime.SpecifyKind(updatedGame.ReleaseDate.Value, DateTimeKind.Utc);
            }

            if (image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                var fileName = Path.GetFileName(image.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                game.ImageUrl = $"/images/{fileName}";
            }

            _context.Entry(game).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
