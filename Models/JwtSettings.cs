/*
 * File: JwtSettings.cs
 * Date: October 11, 2023
 * Description: This file defines the JwtSettings class, which holds JWT configuration settings.
 */

namespace TravelAgency.Models
{
    public class JwtSettings
    {
        public required string Key { get; set; } // The JWT secret key
        public required string Issuer { get; set; } // The issuer of the JWT
        public required string Audience { get; set; } // The audience of the JWT
    }
}
