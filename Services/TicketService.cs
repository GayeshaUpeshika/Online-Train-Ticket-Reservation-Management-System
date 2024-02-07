/*
 * File: TicketService.cs
 * Date: October 11, 2023
 * Description: This file defines the TicketService class, which provides CRUD operations for tickets.
 */

using MongoDB.Driver;
using TravelAgency.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelAgency.Services
{
    public class TicketService
    {
        private readonly IMongoCollection<Ticket> _tickets;

        public TicketService(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _tickets = database.GetCollection<Ticket>("Tickets");
        }

        // Get all tickets
        public List<Ticket> Get() => _tickets.Find(ticket => true).ToList();

        // Get a ticket by ID
        public Ticket Get(string id) => _tickets.Find<Ticket>(ticket => ticket.Id == id).FirstOrDefault();

        // Get tickets by traveler's NIC
        public List<Ticket> GetByNic(string nic) => _tickets.Find(ticket => ticket.TravelerNIC == nic).ToList();

        // Create a new ticket
        public Ticket Create(Ticket ticket)
        {
            // Check reservation date within 30 days from the booking date.
            if (IsValidReservationDate(ticket.ReservationDate))
            {
                // Check maximum 4 reservations per reference ID.
                int existingReservationsCount = (int)_tickets.CountDocuments(t => t.ReferenceID == ticket.ReferenceID);

                if (existingReservationsCount < 4)
                {
                    _tickets.InsertOne(ticket);
                    return ticket;
                }
                else
                {
                    throw new Exception("Maximum 4 reservations per reference ID allowed.");
                }
            }
            else
            {
                throw new Exception("Reservation date must be within 30 days from the booking date.");
            }
        }

        // Update an existing ticket
        public void Update(string id, Ticket ticketIn)
        {
            Ticket existingTicket = Get(id);

            if (existingTicket != null)
            {
                // Check if it's at least 5 days before the reservation date to update.
                if (IsValidUpdateDate(existingTicket.ReservationDate))
                {
                    _tickets.ReplaceOne(ticket => ticket.Id == id, ticketIn);
                }
                else
                {
                    throw new Exception("Reservations can only be updated at least 5 days before the reservation date.");
                }
            }
            else
            {
                throw new Exception("Ticket not found.");
            }
        }

        // Cancel a reservation
        public void CancelReservation(string id)
        {
            Ticket existingTicket = Get(id);

            if (existingTicket != null)
            {
                // Check if it's at least 5 days before the reservation date to cancel.
                if (IsValidUpdateDate(existingTicket.ReservationDate))
                {
                    _tickets.DeleteOne(ticket => ticket.Id == id);
                }
                else
                {
                    throw new Exception("Reservations can only be canceled at least 5 days before the reservation date.");
                }
            }
            else
            {
                throw new Exception("Ticket not found.");
            }
        }

        // Remove a ticket by ID
        public void Remove(string id) => _tickets.DeleteOne(ticket => ticket.Id == id);

        // Get tickets by train ID
        public List<Ticket> GetByTrainId(string trainId) => _tickets.Find(ticket => ticket.TrainId == trainId).ToList();

        // Check if the reservation date is valid (within 30 days from the booking date)
        private bool IsValidReservationDate(DateTime reservationDate)
        {
            DateTime bookingDate = DateTime.Now;
            TimeSpan timeDifference = reservationDate - bookingDate;
            return timeDifference.TotalDays <= 30 && timeDifference.TotalDays >= 0;
        }

        // Check if the update date is valid (at least 5 days before the reservation date)
        private bool IsValidUpdateDate(DateTime reservationDate)
        {
            DateTime currentDate = DateTime.Now;
            TimeSpan timeDifference = reservationDate - currentDate;
            return timeDifference.TotalDays >= 5;
        }
    }
}
