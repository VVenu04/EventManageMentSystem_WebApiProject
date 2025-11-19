using Application.DTOs;
using Application.Interface.IRepo; // Assuming IEventRepo is here
using Application.Interface.IService;
using Application.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Service
{
    public class EventService : IEventService
    {
        private readonly IEventRepo _eventRepo;

        public EventService(IEventRepo eventRepo)
        {
            _eventRepo = eventRepo;
        }

        // --- IMPLEMENTED: ADD EVENT ---
        public async Task<EventDto> AddEventAsync(EventDto eventDTO)
        {
            if (eventDTO == null)
            {
                throw new ArgumentNullException(nameof(eventDTO));
            }

            var @event = EventMapper.MapToEvent(eventDTO);
            var addedEvent = await _eventRepo.AddAsync(@event); // Assumes AddAsync returns the added entity

            return EventMapper.MapToEventDto(addedEvent);
        }

        // --- IMPLEMENTED: DELETE EVENT ---
        public async Task DeleteEventAsync(Guid Id)
        {
            var @event = await _eventRepo.GetByIdAsync(e => e.EventID == Id);
            if (@event != null)
            {
                await _eventRepo.DeleteAsync(@event);
            }
        }

        // --- IMPLEMENTED: GET ALL EVENTS ---
        public async Task<IEnumerable<EventDto>> GetAllAsync()
        {
            var events = await _eventRepo.GetAllAsync();
            return EventMapper.MapToEventDtoList(events); // Use the new mapper helper
        }

        // --- IMPLEMENTED: GET EVENT BY ID ---
        public async Task<EventDto> GetEventAsync(Guid eventId)
        {
            // Assuming IEventRepo has GetByIdAsync(Guid id) 
            var @event = await _eventRepo.GetByIdAsync(eventId);

            if (@event == null)
            {
                // Throw exception or return null based on preferred service pattern
                return null;
            }
            return EventMapper.MapToEventDto(@event);
        }
    }
}