using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameReviewApi.Data;
using GameReviewApi.DTOs; // Assuming you've created this namespace
using GameReviewApi.Models;
using System.Security.Claims;

namespace GameReviewApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly GameReviewContext _context;

        public ReviewsController(GameReviewContext context)
        {
            _context = context;
        }

        [HttpGet("game/{gameId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsForGame(int gameId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.GameId == gameId)
                .Include(r => r.User) // Include the User entity to access the Username
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Comment = r.Comment,
                    Rating = r.Rating,
                    Username = r.User != null ? r.User.Username : "Anonymous"
                })
                .ToListAsync();

            if (!reviews.Any())
            {
                return Ok(new List<ReviewDto>()); // Return an empty list if no reviews found
            }

            return Ok(reviews);
        }

        [HttpPost]
public async Task<ActionResult<ReviewDto>> AddReview([FromBody] Review review)
{
    if (review == null)
    {
        return BadRequest("Review data is missing.");
    }

    // Get the user ID from the authenticated user
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized("User must be logged in to post a review.");
    }

    review.UserId = int.Parse(userId);

    _context.Reviews.Add(review);
    try
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateException ex)
    {
        var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;
        return StatusCode(500, $"An error occurred while saving the review: {innerExceptionMessage}");
    }

    var reviewDto = new ReviewDto
    {
        Id = review.Id,
        Comment = review.Comment,
        Rating = review.Rating,
        Username = (await _context.Users.FindAsync(review.UserId))?.Username ?? "Anonymous"
    };

    // Ensure that the GetReview method exists and is correctly named.
    return CreatedAtAction(nameof(GetReview), new { id = review.Id }, reviewDto);
}


        [HttpGet("review/{id}")]
        public async Task<ActionResult<ReviewDto>> GetReview(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.User) // Include the User entity to access the Username
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            var reviewDto = new ReviewDto
            {
                Id = review.Id,
                GameId = review.GameId,
                Comment = review.Comment,
                Rating = review.Rating,
                Username = review.User != null ? review.User.Username : "Anonymous"
            };

            return Ok(reviewDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(int id, [FromBody] Review updatedReview)
        {
            if (id != updatedReview.Id)
            {
                return BadRequest();
            }

            _context.Entry(updatedReview).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Reviews.Any(r => r.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
