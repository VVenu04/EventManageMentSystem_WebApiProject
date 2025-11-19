using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface IEventService
    {
        Task<EventDto> AddEventAsync(EventDto eventDTO);
        Task DeleteEventAsync(Guid Id);
        Task<EventDto> GetEventAsync(Guid vendorId);
        Task<IEnumerable<EventDto>> GetAllAsync();
    }
}
