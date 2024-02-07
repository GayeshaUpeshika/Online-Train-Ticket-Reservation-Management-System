/*
 * File: User.cs
 * Date: October 11, 2023
 * Description: This file defines the User class.
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TravelAgency.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; } // Unique identifier for the user

        public required string Name { get; set; } // The name of the user
        public required string Email { get; set; } // The email address of the user
        public required string Password { get; set; } // The password of the user
        public required string Role { get; set; } // The role of the user (Backoffice or TravelAgent)
    }
}
