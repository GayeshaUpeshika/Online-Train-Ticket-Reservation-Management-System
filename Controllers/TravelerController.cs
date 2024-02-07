// Comment Header Block for the TravelerController
/*
 * File: TravelerController.cs
 * Date: October 11, 2023
 * Description: This file contains the TravelerController class, which handles traveler-related API endpoints.
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
using System.Threading.Tasks;  // Added for async Task usage

namespace TravelAgency.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TravelerController : ControllerBase
    {
        private readonly TravelerService _travelerService;
        private readonly JwtSettings _jwtSettings;

        public TravelerController(TravelerService travelerService, IOptions<JwtSettings> jwtSettings)
        {
            _travelerService = travelerService;
            _jwtSettings = jwtSettings.Value;
        }

        // GET: api/traveler
        [HttpGet]
        public async Task<List<Traveler>> Get() => await _travelerService.GetAsync();

        [HttpGet("{nic}")]
        public async Task<ActionResult<Traveler>> GetByNIC(string nic)
        {
            // Inline comment for GetByNIC method
            // Get a traveler by NIC (National Identification Card number)
            var traveler = await _travelerService.GetAsync(nic);
            if (traveler == null)
            {
                return NotFound("There is no traveler with this NIC: " + nic);
            }

            return Ok(traveler);
        }

        [HttpPost]
        public ActionResult<Traveler> Create(Traveler traveler)
        {
            // Inline comment for Create method
            // Create a new traveler record
            traveler.Id = null;  // Ensure Id is not set
            _travelerService.Create(traveler);
            return Ok(traveler);
        }

        [HttpPut("{nic}")]
        public async Task<ActionResult> Put(string nic, TravelerUpdate travelerIn)
        {
            // Inline comment for Put method
            // Update an existing traveler's information by NIC
            var traveler = await _travelerService.GetAsync(nic);
            var email = await _travelerService.GetbyEmail(travelerIn.Email);
            if (email != null)
            {
                return BadRequest("Email Already exists!");
            }
            if (traveler == null)
            {
                return NotFound("There is no traveler with this NIC: " + nic);
            }

            travelerIn.Id = traveler.Id;

            await _travelerService.UpdateAsync(nic, travelerIn);

            return Ok("Updated Successfully");
        }

        [HttpDelete("{nic}")]
        public async Task<ActionResult> Delete(string nic)
        {
            // Inline comment for Delete method
            // Delete a traveler record by NIC
            var traveler = await _travelerService.GetAsync(nic);
            if (traveler == null)
            {
                return NotFound("There is no traveler with this NIC: " + nic);
            }

            await _travelerService.RemoveAsync(nic);

            return Ok("Deleted Successfully");
        }

        [HttpPut("status/{nic}")]
        public IActionResult ActivateDeactivate(string nic, bool status)
        {
            // Inline comment for ActivateDeactivate method
            // Activate or deactivate a traveler by NIC
            _travelerService.ActivateDeactivateTraveler(nic, status);
            return NoContent();
        }

        [HttpPost("register")]
        public ActionResult<Traveler> Register(Traveler user)
        {
            // Inline comment for Register method
            // Register a new traveler user
            // Validate email format
            var emailRegex = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$";
            if (!Regex.IsMatch(user.Email, emailRegex))
            {
                return BadRequest(new { message = "Invalid email format" });
            }
            // Check if email is already registered
            var existingUser = _travelerService.GetByEmail(user.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already registered" });
            }

            user.Id = null;  // Ensure Id is not set
            user.IsActive = true;
            user.Password = TravelerService.HashPassword(user.Password); // Hash user password
            _travelerService.Create(user);
            return CreatedAtRoute("GetUser", new { id = user.Id.ToString() }, user);
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest loginRequest)
        {
            // Inline comment for Login method
            // Authenticate a traveler user and generate a JWT token
            var user = _travelerService.Authenticate(loginRequest.Email, loginRequest.Password);

            if (user == null)
                return BadRequest(new { message = "Email or password is incorrect" });
            Console.WriteLine(user.Id);
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                user.Id,
                user.Email,
                user.IsActive,
                Token = token
            });
        }

        private string GenerateJwtToken(Traveler user)
        {
            // Inline comment for GenerateJwtToken method
            // Generate a JWT token for the authenticated user
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("NIC", user.NIC),
                new Claim("LastName", user.LastName),
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
