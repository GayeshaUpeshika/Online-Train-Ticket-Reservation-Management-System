/*
 * File: Train.cs
 * Date: October 11, 2023
 * Description: This file defines the Train class.
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TravelAgency.Models
{
    // Enum to represent the status of a train
    public enum TrainStatus
    {
        Inactive,   // The train is inactive
        Active,     // The train is active
        Published   // The train is published
    }

    public class Train
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; } // Unique identifier for the train

        public required string TrainName { get; set; } // The name of the train
        public required ScheduleItem[] Schedule { get; set; } // An array of schedule items for the train
        public TrainStatus Status { get; set; } // The status of the train
    }
}
