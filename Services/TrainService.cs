/*
 * File: TrainServices.cs
 * Date: October 11, 2023
 * Description: This file defines the TrainServices class, which provides CRUD operations for trains.
 */

using MongoDB.Driver;
using TravelAgency.Models;
using System.Collections.Generic;
using System.Linq;

namespace TravelAgency.Services
{
    public class TrainServices
    {
        private readonly IMongoCollection<Train> _trains;
        private readonly TicketService _ticketService;

        // Constructor for TrainServices
        public TrainServices(IDatabaseSettings settings, TicketService ticketService)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _trains = database.GetCollection<Train>("Trains");
            _ticketService = ticketService;
        }

        // Get a list of all trains
        public List<Train> Get() => _trains.Find(train => true).ToList();

        // Get a train by its ID
        public Train Get(string id) => _trains.Find<Train>(train => train.Id == id).FirstOrDefault();

        // Create a new train
        public Train Create(Train train)
        {
            _trains.InsertOne(train);
            return train;
        }

        // Update a train by its ID
        public void Update(string id, Train trainIn) => _trains.ReplaceOne(train => train.Id == id, trainIn);

        // Remove a train by its ID
        public void Remove(string id) => _trains.DeleteOne(train => train.Id == id);

        // Check if a train has reservations
        public bool HasReservations(string trainId) => _ticketService.GetByTrainId(trainId).Any();
    }
}
