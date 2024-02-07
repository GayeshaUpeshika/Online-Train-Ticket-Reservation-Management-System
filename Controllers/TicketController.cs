/*
 * File Name: TicketController.cs
 * Description: Controller responsible for handling operations related to Tickets.
 */
using Microsoft.AspNetCore.Mvc;
using TravelAgency.Models;
using TravelAgency.Services;
using System;
using System.Collections.Generic;

namespace TravelAgency.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly TicketService _ticketService;

        /// <summary>
        /// Constructor that initializes the TicketService.
        /// </summary>
        /// <param name="ticketService">Service for ticket related operations.</param>
        public TicketController(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        /// <summary>
        /// Retrieves a list of all tickets.
        /// </summary>
        /// <returns>List of tickets.</returns>
        [HttpGet]
        public ActionResult<List<Ticket>> Get() => _ticketService.Get();

        /// <summary>
        /// Retrieves a specific ticket by its ID.
        /// </summary>
        /// <param name="id">Ticket ID.</param>
        /// <returns>The specified ticket or NotFound if not present.</returns>
        [HttpGet("{id:length(24)}", Name = "GetTicket")]
        public ActionResult<Ticket> Get(string id)
        {
            var ticket = _ticketService.Get(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return ticket;
        }

        /// <summary>
        /// Retrieves a ticket based on NIC.
        /// </summary>
        /// <param name="nic">NIC for the ticket search.</param>
        /// <returns>The ticket associated with the NIC or NotFound if not present.</returns>
        [HttpGet("{nic}", Name = "GetTicketByNic")]
        public ActionResult<Ticket> GetByNic(string nic)
        {
            var ticket = _ticketService.GetByNic(nic);
            if (ticket == null)
            {
                return NotFound();
            }
            return Ok(ticket);
        }

        /// <summary>
        /// Creates a new ticket.
        /// </summary>
        /// <param name="ticket">The ticket to create.</param>
        /// <returns>The created ticket with its ID.</returns>
        [HttpPost]
        public ActionResult<Ticket> Create(Ticket ticket)
        {
            ticket.Id = null;  // Ensure Id is not set
            try
            {
                var createdTicket = _ticketService.Create(ticket);
                return CreatedAtRoute("GetTicket", new { id = createdTicket.Id.ToString() }, createdTicket);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing ticket by its ID.
        /// </summary>
        /// <param name="id">Ticket ID.</param>
        /// <param name="ticketIn">Updated ticket details.</param>
        /// <returns>NoContent if successful, NotFound if ticket doesn't exist or BadRequest if an error occurs.</returns>
        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Ticket ticketIn)
        {
            var ticket = _ticketService.Get(id);
            if (ticket == null)
            {
                return NotFound();
            }
            try
            {
                _ticketService.Update(id, ticketIn);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cancels (deletes) a reservation based on its ID.
        /// </summary>
        /// <param name="id">Ticket ID.</param>
        /// <returns>NoContent if successful, NotFound if ticket doesn't exist or BadRequest if an error occurs.</returns>
        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var ticket = _ticketService.Get(id);
            if (ticket == null)
            {
                return NotFound();
            }
            try
            {
                _ticketService.CancelReservation(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
