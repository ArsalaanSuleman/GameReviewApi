using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using GameReviewApi.Models;
using GameReviewApi.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace GameReviewApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly GameReviewContext _context;

        public UserController(GameReviewContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                return BadRequest("Username already exists");
            }

            user.PasswordHash = HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User login)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == login.Username);

            if (user == null || !VerifyPasswordHash(login.PasswordHash, user.PasswordHash))
            {
                return Unauthorized("Invalid username or password");
            }

            // Generate a JWT token and return it
            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool VerifyPasswordHash(string enteredPassword, string storedHash)
        {
            var enteredHash = HashPassword(enteredPassword);
            return enteredHash == storedHash;
        }

        private string GenerateJwtToken(User user)
        {
            // Define the token handler
            var tokenHandler = new JwtSecurityTokenHandler();
    
            // Define the key for signing the token (use a secure key in a real application)
            var key = Encoding.UTF8.GetBytes("A9df34FeLksdf34Df39Ls9df34FeLksd"); // replace with a more secure key from configuration
    
            // Define the token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] 
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Ensure this is the correct user ID
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Set token expiration
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = "localhost",
                Audience = "localhost"
            };

            // Create the token
            var token = tokenHandler.CreateToken(tokenDescriptor);
    
            // Return the generated token
            return tokenHandler.WriteToken(token);
        }

        [HttpGet("profile")]
public async Task<IActionResult> GetProfile()
{
    // Extract user ID from the JWT token
    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
    {
        return Unauthorized("Invalid user ID");
    }

    // Fetch the user from the database
    var user = await _context.Users.FindAsync(userId);

    if (user == null)
    {
        return NotFound("User not found");
    }

    // Return the user's profile information
    return Ok(new { user.Username, user.Name, user.Email });
}

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] User updatedUser)
        {
            // Extract user ID from the JWT token
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user ID");
            }

            // Fetch the user from the database
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Update the user's information
            user.Username = updatedUser.Username;
            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;

            // If the password is being updated, hash it
            if (!string.IsNullOrEmpty(updatedUser.PasswordHash))
            {
                user.PasswordHash = HashPassword(updatedUser.PasswordHash);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok("Profile updated successfully");
        }
    }
}
