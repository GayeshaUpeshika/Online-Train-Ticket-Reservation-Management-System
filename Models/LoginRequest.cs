/*
 * File: LoginRequest.cs
 * Date: October 11, 2023
 * Description: This file defines the LoginRequest class used for user login.
 */

namespace TravelAgency.Models
{
    // Class to represent a user login request
    public class LoginRequest
    {
        public required string Email { get; set; } // The email address of the user
        public required string Password { get; set; } // The user's password
    }
}
