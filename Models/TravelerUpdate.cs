/*
 * File: TravelerUpdate.cs
 * Date: October 11, 2023
 * Description: This file defines the TravelerUpdate class.
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TravelAgency.Models
{
    public class TravelerUpdate
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty; // Unique identifier for the traveler update

        public required string FirstName { get; set; } // The first name of the traveler
        public required string LastName { get; set; } // The last name of the traveler
        public required string Email { get; set; } // The email address of the traveler
        public string? Password { get; set; } // The password of the traveler (nullable)
        public string? NIC { get; set; } // The NIC (National Identity Card) of the traveler (nullable)
        public bool? IsActive { get; set; } // Indicates if the traveler is active (nullable)
    }
}
