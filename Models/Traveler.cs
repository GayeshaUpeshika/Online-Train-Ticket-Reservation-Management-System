/*
 * File: Traveler.cs
 * Date: October 11, 2023
 * Description: This file defines the Traveler class.
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TravelAgency.Models
{
    public class Traveler
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty; // Unique identifier for the traveler

        public required string FirstName { get; set; } // The first name of the traveler
        public required string LastName { get; set; } // The last name of the traveler
        public required string Email { get; set; } // The email address of the traveler

        public required string Password { get; set; } // The password of the traveler
        public required string NIC { get; set; } // The National Identity Card (NIC) of the traveler
        public bool IsActive { get; set; } // Indicates whether the traveler is active
    }
}
