
using Application.DTOs;
using Application.Interface.IService;
using Application.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [HttpPost("AddEvent")]
        public async Task<IActionResult> AddEvent([FromBody] EventDto eventDTO)
        {
            if (!ModelState.IsValid)
                return Ok(eventDTO);

            if (eventDTO == null)
            {
                return BadRequest("fill panela");
            }
            var addedEvent = await _eventService.AddEventAsync(eventDTO);
            return Ok(addedEvent);
        }

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [HttpDelete("DeleteEvent")]
        public async Task<IActionResult> DeleteEvent(Guid Id)
        {
            if (Id == Guid.Empty) { return BadRequest("id ela"); }
            await _eventService.DeleteEventAsync(Id);
            return Ok();
        }

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [HttpGet("1Event")]
        public async Task<IActionResult> GetEventById(Guid eventId)
        {
            var @event = await _eventService.GetEventAsync(eventId);
            if (@event == null) return NotFound();
            return Ok(@event);
        }
        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [HttpGet("AllEvent")]
        public async Task<IActionResult> GetAllAsync()
        {
            var events = await _eventService.GetAllAsync();
            if (events == null) return NotFound();
            return Ok(events);
        }
    }
}
