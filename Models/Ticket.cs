/*
 * File: Ticket.cs
 * Date: October 11, 2023
 * Description: This file defines the Ticket class.
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TravelAgency.Models
{
    public class Ticket
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty; // Unique identifier for the ticket

        public required string TravelerNIC { get; set; } // National ID of the traveler
        public required string TrainId { get; set; } // ID of the associated train
        public required string ScheduleId { get; set; } // ID of the associated schedule
        public DateTime ReservationDate { get; set; } // Date when the ticket was reserved
        public required string ReferenceID { get; set; } // Reference ID for the ticket
    }
}
