// Comment header block for the TrainController.cs file
/*
 * File: TrainController.cs
 * Date: October 11, 2023
 * Description: This file contains the implementation of the TrainController, which handles train-related API endpoints.
 */
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TravelAgency.Models;
using TravelAgency.Services;
using MongoDB.Bson;
using System.Collections.Generic;

namespace TravelAgency.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainController : ControllerBase
    {
        private readonly TrainServices _trainService;

        public TrainController(TrainServices trainService)
        {
            _trainService = trainService;
        }

        [HttpGet]
        // Inline comment for the Get method
        // Retrieves a list of all trains.
        public ActionResult<List<Train>> Get() => _trainService.Get();

        [HttpGet("{id:length(24)}", Name = "GetTrain")]
        // Inline comment for the Get method with parameter
        // Retrieves a specific train by its ID.
        public ActionResult<Train> Get(string id)
        {
            var train = _trainService.Get(id);
            if (train == null)
            {
                return NotFound();
            }
            return train;
        }

        [HttpPost]
        // Inline comment for the Create method
        // Creates a new train.
        public ActionResult<Train> Create(Train train)
        {
            if (train.Schedule == null || !train.Schedule.Any())
            {
                return BadRequest("Train schedule cannot be empty.");
            }

            foreach (var scheduleItem in train.Schedule)
            {
                if (string.IsNullOrEmpty(scheduleItem.Origin) || string.IsNullOrEmpty(scheduleItem.Destination))
                {
                    return BadRequest("Origin and Destination cannot be empty in the schedule.");
                }
                if (scheduleItem.OriginTime > scheduleItem.DestinationTime)
                {
                    return BadRequest("Origin time cannot be after Destination time.");
                }
                scheduleItem.Id = ObjectId.GenerateNewId().ToString(); // Auto-generate an ID for each ScheduleItem
            }

            if (!Enum.IsDefined(typeof(TrainStatus), train.Status))
            {
                return BadRequest("Invalid TrainStatus value.");
            }

            train.Id = null;
            _trainService.Create(train);
            return CreatedAtRoute("GetTrain", new { id = train.Id.ToString() }, train);
        }

        [HttpPut("{id:length(24)}")]
        // Inline comment for the Update method
        // Updates an existing train by its ID.
        public IActionResult Update(string id, Train trainIn)
        {
            var train = _trainService.Get(id);
            if (train == null)
            {
                return NotFound();
            }

            if (train.Status == TrainStatus.Published &&
                !train.Schedule.SequenceEqual(trainIn.Schedule, new ScheduleItemComparer()))
            {
                return BadRequest("Cannot update schedules for a published train.");
            }

            _trainService.Update(id, trainIn);
            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        // Inline comment for the Delete method
        // Deletes an existing train by its ID.
        public IActionResult Delete(string id)
        {
            var train = _trainService.Get(id);
            if (train == null)
            {
                return NotFound();
            }

            if (_trainService.HasReservations(id))
            {
                return BadRequest("Cannot cancel a train with existing reservations.");
            }

            _trainService.Remove(id);
            return NoContent();
        }

        [HttpPost("{id:length(24)}/append-schedule")]
        // Inline comment for the AppendSchedule method
        // Appends a new schedule to an existing train.
        public IActionResult AppendSchedule(string id, ScheduleItem scheduleItem)
        {
            var train = _trainService.Get(id);
            if (train == null)
            {
                return NotFound();
            }

            if (train.Status == TrainStatus.Published)
            {
                return BadRequest("Cannot append schedules to a published train.");
            }

            if (string.IsNullOrEmpty(scheduleItem.Origin) || string.IsNullOrEmpty(scheduleItem.Destination))
            {
                return BadRequest("Origin and Destination cannot be empty in the schedule.");
            }

            if (scheduleItem.OriginTime > scheduleItem.DestinationTime)
            {
                return BadRequest("Origin time cannot be after Destination time.");
            }

            scheduleItem.Id = ObjectId.GenerateNewId().ToString(); // Auto-generate an ID for the ScheduleItem

            // Append the new schedule to the train's existing schedules
            var updatedSchedules = train.Schedule.ToList();
            updatedSchedules.Add(scheduleItem);
            train.Schedule = updatedSchedules.ToArray();

            // Update the train in the database
            _trainService.Update(id, train);

            return Ok(train);
        }

        [HttpDelete("{id:length(24)}/delete-schedule/{scheduleId:length(24)}")]
        // Inline comment for the DeleteSchedule method
        // Deletes a specific schedule from an existing train.
        public IActionResult DeleteSchedule(string id, string scheduleId)
        {
            var train = _trainService.Get(id);
            if (train == null)
            {
                return NotFound();
            }

            if (train.Status == TrainStatus.Published)
            {
                return BadRequest("Cannot delete schedules from a published train.");
            }

            var matchingSchedule = train.Schedule.FirstOrDefault(s => s.Id == scheduleId);

            if (matchingSchedule == null)
            {
                return BadRequest("The specified schedule was not found in the train.");
            }

            // Remove the matching schedule from the train's schedules
            var updatedSchedules = train.Schedule.ToList();
            updatedSchedules.Remove(matchingSchedule);
            train.Schedule = updatedSchedules.ToArray();

            // Update the train in the database
            _trainService.Update(id, train);

            return Ok(train);
        }

        [HttpPut("{id:length(24)}/edit-train")]
        // Inline comment for the EditTrain method
        // Edits the details of an existing train.
        public IActionResult EditTrain(string id, Train trainToUpdate)
        {
            var currentTrain = _trainService.Get(id);
            if (currentTrain == null)
            {
                return NotFound();
            }

            if (currentTrain.Status == TrainStatus.Published)
            {
                return BadRequest("Cannot modify details of a published train.");
            }

            if (!Enum.IsDefined(typeof(TrainStatus), trainToUpdate.Status))
            {
                return BadRequest("Invalid TrainStatus value.");
            }

            // Ensure we keep the current train's schedule
            trainToUpdate.Schedule = currentTrain.Schedule;
            trainToUpdate.Id = id; // Make sure we are updating the correct train.

            _trainService.Update(id, trainToUpdate);

            return NoContent();
        }

        [HttpPut("{trainId:length(24)}/edit-schedule/{scheduleId:length(24)}")]
        // Inline comment for the EditSchedule method
        // Edits a specific schedule item within an existing train.
        public IActionResult EditSchedule(string trainId, string scheduleId, ScheduleItem updatedScheduleItem)
        {
            var train = _trainService.Get(trainId);
            if (train == null)
            {
                return NotFound();
            }

            if (train.Status == TrainStatus.Published)
            {
                return BadRequest("Cannot edit schedules of a published train.");
            }

            if (string.IsNullOrEmpty(updatedScheduleItem.Origin) || string.IsNullOrEmpty(updatedScheduleItem.Destination))
            {
                return BadRequest("Origin and Destination cannot be empty in the schedule.");
            }

            if (updatedScheduleItem.OriginTime > updatedScheduleItem.DestinationTime)
            {
                return BadRequest("Origin time cannot be after Destination time.");
            }

            var existingScheduleItem = train.Schedule.FirstOrDefault(s => s.Id == scheduleId);

            if (existingScheduleItem == null)
            {
                return BadRequest("The specified schedule was not found in the train.");
            }

            // Replace the existing schedule with the updated one
            int index = Array.IndexOf(train.Schedule, existingScheduleItem);
            updatedScheduleItem.Id = scheduleId;  // Ensure the ID remains the same
            train.Schedule[index] = updatedScheduleItem;

            // Update the train in the database
            _trainService.Update(trainId, train);

            return Ok(train);
        }

        [HttpGet("{trainId:length(24)}/schedule/{scheduleId:length(24)}")]
        // Inline comment for the GetSchedule method
        // Retrieves a specific schedule item within a train by their IDs.
        public ActionResult<ScheduleItem> GetSchedule(string trainId, string scheduleId)
        {
            var train = _trainService.Get(trainId);
            if (train == null)
            {
                return NotFound("Train not found.");
            }

            var scheduleItem = train.Schedule.FirstOrDefault(s => s.Id == scheduleId);

            if (scheduleItem == null)
            {
                return NotFound("Schedule not found within the specified train.");
            }

            return Ok(scheduleItem);
        }
    }
}
