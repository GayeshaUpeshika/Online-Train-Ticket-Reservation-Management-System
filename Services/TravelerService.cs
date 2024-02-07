/*
 * File: TravelerService.cs
 * Date: October 11, 2023
 * Description: This file contains the TravelerService class, which provides CRUD operations for Traveler entities.
 */

using MongoDB.Driver;
using TravelAgency.Models;
using System.Collections.Generic;
using System.Linq;
using BCrypt.Net;
using System.Threading.Tasks;

namespace TravelAgency.Services
{
    public class TravelerService
    {
        private readonly IMongoCollection<Traveler> _travelers;
        private readonly IMongoCollection<TravelerUpdate> _travelersUpdate;

        public TravelerService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _travelers = database.GetCollection<Traveler>("Travelers");
            _travelersUpdate = database.GetCollection<TravelerUpdate>("Travelers");
        }

        // Get all travelers
        public async Task<List<Traveler>> GetAsync()
        {
            return await _travelers.Find(_ => true).ToListAsync();
        }

        // Get traveler by NIC
        public async Task<Traveler> GetAsync(string nic)
        {
            return await _travelers.Find(x => x.NIC == nic).FirstOrDefaultAsync();
        }

        // Get traveler by NIC
        public async Task<Traveler> GetbyEmail(string email)
        {
            return await _travelers.Find(x => x.Email == email).FirstOrDefaultAsync();
        }

        // Create a new traveler
        public Traveler Create(Traveler traveler)
        {
            _travelers.InsertOne(traveler);
            return traveler;
        }

        // Update traveler by NIC
        public async Task UpdateAsync(string nic, TravelerUpdate travelerIn)
        {
            var updateBuilder = Builders<TravelerUpdate>.Update;
            var updates = new List<UpdateDefinition<TravelerUpdate>>();

            if (!string.IsNullOrEmpty(travelerIn.FirstName))
                updates.Add(updateBuilder.Set(t => t.FirstName, travelerIn.FirstName));

            if (!string.IsNullOrEmpty(travelerIn.LastName))
                updates.Add(updateBuilder.Set(t => t.LastName, travelerIn.LastName));

            if (!string.IsNullOrEmpty(travelerIn.Email))
                updates.Add(updateBuilder.Set(t => t.Email, travelerIn.Email));

            if (travelerIn.IsActive.HasValue)
                updates.Add(updateBuilder.Set(t => t.IsActive, travelerIn.IsActive));

            // Only update Password if it's not null
            if (!string.IsNullOrEmpty(travelerIn.Password))
                updates.Add(updateBuilder.Set(t => t.Password, HashPassword(travelerIn.Password)));

            // Only update NIC if it's not null
            if (!string.IsNullOrEmpty(travelerIn.NIC))
                updates.Add(updateBuilder.Set(t => t.NIC, travelerIn.NIC));

            var combinedUpdate = updateBuilder.Combine(updates);

            await _travelersUpdate.UpdateOneAsync(x => x.NIC == nic, combinedUpdate);
        }

        // Delete traveler by NIC
        public async Task RemoveAsync(string nic)
        {
            await _travelers.DeleteOneAsync(x => x.NIC == nic);
        }

        // Activate or deactivate traveler by NIC
        public void ActivateDeactivateTraveler(string nic, bool status)
        {
            var traveler = _travelers.Find(x => x.NIC == nic).FirstOrDefault();
            if (traveler != null)
            {
                traveler.IsActive = status;
                _travelers.ReplaceOne(x => x.NIC == nic, traveler);
            }
        }

        // Get traveler by email
        public Traveler GetByEmail(string email)
        {
            return _travelers.Find<Traveler>(x => x.Email == email).FirstOrDefault();
        }

        // Authenticate traveler by email and password
        public Traveler Authenticate(string email, string password)
        {
            var user = _travelers.Find<Traveler>(x => x.Email == email).FirstOrDefault();

            // Check if user exists and password is correct
            if (user == null || !VerifyPasswordHash(password, user.Password))
                return null;
            else
                // Authentication successful
                return user;
        }

        // Hash a password
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Verify a password hash
        public static bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
    }
}
