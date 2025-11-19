using Application.Common;
using Application.DTOs;
using Application.Interface.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System; 
using System.Collections.Generic; 
using System.Linq; 

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        //  AddEvent Method

        [ProducesErrorResponseType(typeof(ApiResponse<EventDto>))]
        [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status201Created)]
        [HttpPost("AddEvent")]
        public async Task<IActionResult> AddEvent([FromBody] EventDto eventDTO)
        {
            //  Validation Check (ModelState)
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<EventDto>.Failure("Validation failed.", validationErrors));
            }

            //  Null Check 
            if (eventDTO == null)
            {
                return BadRequest(ApiResponse<EventDto>.Failure("Event data cannot be null."));
            }

            try
            {
                var addedEvent = await _eventService.AddEventAsync(eventDTO);

                //  Success Response (201 Created)
                return CreatedAtAction(
                    nameof(GetEventById),
                    new { eventId = addedEvent.EventID },
                    ApiResponse<EventDto>.Success(addedEvent, "Event created successfully.")
                );
            }
            catch (Exception ex)
            {
                // Catches errors from the service layer
                return StatusCode(500, ApiResponse<EventDto>.Failure(ex.Message));
            }
        }

        // DeleteEvent Method 

        [ProducesErrorResponseType(typeof(ApiResponse<object>))]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [HttpDelete("DeleteEvent")]
        public async Task<IActionResult> DeleteEvent(Guid Id)
        {
            //  Fixed Guid Check 
            if (Id == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Failure("A valid Event ID is required."));
            }

            try
            {
                await _eventService.DeleteEventAsync(Id);
                return Ok(ApiResponse<object>.Success(null, "Event deleted successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failure(ex.Message));
            }
        }

        //  GetEventById Method 

        [ProducesErrorResponseType(typeof(ApiResponse<EventDto>))]
        [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status200OK)]
        [HttpGet("1Event")]
        public async Task<IActionResult> GetEventById(Guid eventId)
        {
            if (eventId == Guid.Empty)
            {
                return BadRequest(ApiResponse<EventDto>.Failure("A valid Event ID is required."));
            }

            var @event = await _eventService.GetEventAsync(eventId);

            //  Not Found Check
            if (@event == null)
            {
                return NotFound(ApiResponse<EventDto>.Failure("Event not found."));
            }

            //  Success Response
            return Ok(ApiResponse<EventDto>.Success(@event));
        }

        //  GetAllAsync Method 

        [ProducesErrorResponseType(typeof(ApiResponse<IEnumerable<EventDto>>))]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EventDto>>), StatusCodes.Status200OK)]
        [HttpGet("AllEvent")]
        public async Task<IActionResult> GetAllAsync()
        {
            var events = await _eventService.GetAllAsync();

            //  Empty List Handling
            if (events == null || !events.Any())
            {
                return Ok(ApiResponse<IEnumerable<EventDto>>.Success(new List<EventDto>(), "No events found."));
            }

            //  Success Response
            return Ok(ApiResponse<IEnumerable<EventDto>>.Success(events));
        }
    }
}