using Application.DTOs;
using Application.Interface.IRepo;
using Application.Interface.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public Task<EventDto> AddEventAsync(EventDto eventDTO)
        {
            throw new NotImplementedException();
        }

        public Task DeleteEventAsync(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<EventDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<EventDto> GetEventAsync(Guid vendorId)
        {
            throw new NotImplementedException();
        }
    }
}
