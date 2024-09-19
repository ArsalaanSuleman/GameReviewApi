using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameReviewApi.Data;
using GameReviewApi.Models;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authorization;


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
                .Include(r => r.User)
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
                return Ok(new List<ReviewDto>()); 
            }

            return Ok(reviews);
        }

[Authorize]
[HttpPost]
public async Task<ActionResult<ReviewDto>> AddReview([FromBody] Review review)
{
    if (review == null)
    {
        Console.WriteLine("Review is null");
        return BadRequest("Review data is missing.");
    }

    Console.WriteLine($"Review received: GameId: {review.GameId}, Comment: {review.Comment}, Rating: {review.Rating}");

    foreach (var claim in User.Claims)
    {
        Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}"); // <--- Place this code here
    }

    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userIdClaim))
    {
        return Unauthorized("User must be logged in to post a review.");
    }

    if (!int.TryParse(userIdClaim, out int userId))
    {
        return BadRequest("Invalid user ID.");
    }

    review.UserId = userId;

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

    var username = (await _context.Users.FindAsync(userId))?.Username;

    var reviewDto = new ReviewDto
    {
        Id = review.Id,
        Comment = review.Comment,
        Rating = review.Rating,
        Username = username
    };

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
    // Retrieve the existing review from the database, including the associated User
    var review = await _context.Reviews
        .Include(r => r.User) // Ensure the User entity is included
        .FirstOrDefaultAsync(r => r.Id == id);
        
    if (review == null)
    {
        return NotFound(); // Return 404 if review not found
    }

    // Update only the fields that are allowed to be updated
    review.Comment = updatedReview.Comment;
    review.Rating = updatedReview.Rating;

    // Save the changes
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

    // Prepare the updated review data, including the Username
    var reviewDto = new ReviewDto
    {
        Id = review.Id,
        Comment = review.Comment,
        Rating = review.Rating,
        Username = review.User?.Username // Ensure the username is included
    };

    return Ok(reviewDto); // Return the updated review object
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
