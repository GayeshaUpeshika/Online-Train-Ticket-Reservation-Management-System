// Comment Header Block for UserController.cs
/*
 * File: UserController.cs
 * Date: October 11, 2023
 * Description: This file contains the UserController class, which handles user-related API endpoints.
 */
using Microsoft.AspNetCore.Mvc;
using TravelAgency.Models;
using TravelAgency.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Security.Claims;

namespace TravelAgency.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly JwtSettings _jwtSettings;

        public UserController(UserService userService, IOptions<JwtSettings> jwtSettings)
        {
            _userService = userService;
            _jwtSettings = jwtSettings.Value;
        }

        // Get method
        [HttpGet]
        public ActionResult<List<User>> Get() => _userService.Get();

        // Get method with id parameter
        [HttpGet("{id:length(24)}", Name = "GetUser")]
        public ActionResult<User> Get(string id)
        {
            var user = _userService.Get(id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        // Create method
        [HttpPost]
        public ActionResult<User> Create(User user)
        {
            user.Id = null;  // Ensure Id is not set
            _userService.Create(user);
            return CreatedAtRoute("GetUser", new { id = user.Id.ToString() }, user);
        }

        // Update method
        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, User userIn)
        {
            var user = _userService.Get(id);
            if (user == null)
            {
                return NotFound();
            }
            _userService.Update(id, userIn);
            return NoContent();
        }

        // Delete method
        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var user = _userService.Get(id);
            if (user == null)
            {
                return NotFound();
            }
            _userService.Remove(user.Id);
            return NoContent();
        }

        // Register method
        [HttpPost("register")]
        public ActionResult<User> Register(User user)
        {
            // Validate email format
            var emailRegex = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$";
            if (!Regex.IsMatch(user.Email, emailRegex))
            {
                return BadRequest(new { message = "Invalid email format" });
            }
            // Check if email is already registered
            var existingUser = _userService.GetByEmail(user.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already registered" });
            }

            // Validate role
            if (user.Role != "Backoffice" && user.Role != "TravelAgent")
            {
                return BadRequest(new { message = "Invalid role. Role must be either 'Backoffice' or 'TravelAgent'." });
            }

            user.Id = null;  // Ensure Id is not set
            user.Password = UserService.HashPassword(user.Password); // Hash user password
            _userService.Create(user);
            return CreatedAtRoute("GetUser", new { id = user.Id.ToString() }, user);
        }

        // Login method
        [HttpPost("login")]
        public IActionResult Login(LoginRequest loginRequest)
        {
            var user = _userService.Authenticate(loginRequest.Email, loginRequest.Password);

            if (user == null)
                return BadRequest(new { message = "Email or password is incorrect" });
            Console.WriteLine(user.Id);
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                user.Id,
                user.Email,
                user.Role,
                Token = token
            });
        }

        // GenerateJwtToken method
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Name),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
                // ... add other claims as needed
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
