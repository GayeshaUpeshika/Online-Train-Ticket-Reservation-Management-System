/*
 * File: UserService.cs
 * Date: October 11, 2023
 * Description: This file defines the UserService class, which handles user-related operations.
 */

using MongoDB.Driver;
using TravelAgency.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using BCrypt.Net;

namespace TravelAgency.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _users = database.GetCollection<User>(settings.UsersCollectionName);
        }

        // Get a list of all users
        public List<User> Get() => _users.Find(user => true).ToList();

        // Get a user by their ID
        public User Get(string id) => _users.Find<User>(user => user.Id == id).FirstOrDefault();

        // Create a new user
        public User Create(User user)
        {
            _users.InsertOne(user);
            return user;
        }

        // Update a user's information by ID
        public void Update(string id, User userIn) => _users.ReplaceOne(user => user.Id == id, userIn);

        // Remove a user by their ID
        public void Remove(string id) => _users.DeleteOne(user => user.Id == id);

        // Get a user by email
        public User GetByEmail(string email)
        {
            return _users.Find<User>(user => user.Email == email).FirstOrDefault();
        }

        // Authenticate a user by email and password
        public User Authenticate(string email, string password)
        {
            var user = _users.Find<User>(user => user.Email == email).FirstOrDefault();

            // Check if user exists and password is correct
            if (user == null || !VerifyPasswordHash(password, user.Password))
                return null;
            else
                // Authentication successful
                return user;
        }

        // Hash a plain text password
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Verify a plain text password against a hashed password
        public static bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
    }
}
