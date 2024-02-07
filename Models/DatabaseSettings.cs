/*
 * File: DatabaseSettings.cs
 * Date: October 11, 2023
 * Description: This file defines the DatabaseSettings class implementing IDatabaseSettings.
 */

namespace TravelAgency.Models
{
    // Class to represent database settings
    public class DatabaseSettings : IDatabaseSettings
    {
        public required string UsersCollectionName { get; set; } // Name of the users collection
        public required string ConnectionString { get; set; } // Connection string to the database
        public required string DatabaseName { get; set; } // Name of the database
    }

    // Interface defining database settings properties
    public interface IDatabaseSettings
    {
        string UsersCollectionName { get; set; } // Name of the users collection
        string ConnectionString { get; set; } // Connection string to the database
        string DatabaseName { get; set; } // Name of the database
    }
}
