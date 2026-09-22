using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (dto.Role != "Organiser" && dto.Role != "Participant")
            {
                return BadRequest("Role must be Organiser or Participant.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("A user with this email already exists.");
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Role = dto.Role,
                PhoneNumber = dto.PhoneNumber,
                DateRegistered = DateTime.Now
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful."
            });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid email or password.");
            }

            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Role", user.Role);

            return Ok(new
            {
                message = "Login successful.",
                userID = user.UserID,
                role = user.Role
            });
        }

        // POST: api/Auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return Ok(new
            {
                message = "Logout successful."
            });
        }
    }
}
