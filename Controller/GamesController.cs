using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameReviewApi.Data;
using GameReviewApi.Models;
using System.IO;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Game>>> GetGames()
        {
            var games = await _context.Games.ToListAsync();
            // Modify the games to return only the relative paths for image URLs
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
        public async Task<ActionResult<Game>> GetGame(int id)
        {
            var game = await _context.Games.FindAsync(id);

            if (game == null)
            {
                return NotFound();
            }

            // Return game with relative image URL
            return Ok(new
            {
                game.Id,
                game.Title,
                game.Genre,
                game.Description,
                game.ReleaseDate,
                ImageUrl = $"/images/{Path.GetFileName(game.ImageUrl)}"
            });
        }

        [HttpPost]
        public async Task<ActionResult<Game>> PostGame([FromForm] Game game, [FromForm] IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                var fileName = Path.GetFileName(image.FileName); // Get the file name
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Create directory if it does not exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Save only the relative path
                game.ImageUrl = $"/images/{fileName}";
            }

            if (game.ReleaseDate.HasValue)
            {
                game.ReleaseDate = DateTime.SpecifyKind(game.ReleaseDate.Value, DateTimeKind.Utc);
            }

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGame), new { id = game.Id }, game);
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

    }
}
