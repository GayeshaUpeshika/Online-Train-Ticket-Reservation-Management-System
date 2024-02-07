/*
 * File: ScheduleItem.cs
 * Date: October 11, 2023
 * Description: This file defines the ScheduleItem class.
 */

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace TravelAgency.Models
{
    public class ScheduleItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } // Unique identifier for the schedule item

        public string Origin { get; set; } // The origin of the schedule item
        public DateTime OriginTime { get; set; } // The time at the origin
        public string Destination { get; set; } // The destination of the schedule item
        public DateTime DestinationTime { get; set; } // The time at the destination
    }
}
