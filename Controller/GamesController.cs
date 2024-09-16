using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameReviewApi.Data;
using GameReviewApi.Models;
using System.IO;
using GameReviewApi.DTOs;
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
