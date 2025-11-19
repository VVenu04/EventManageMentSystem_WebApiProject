using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapper
{
    public class EventMappper
    {
        public static Event? MapToEvent(EventDto eventDto)
        {
            if (eventDto == null)
                return null;
            Event @event = new Event();
            @event.EventName = eventDto.EventName;
            return @event;
        }

        public static EventDto? MapToEventDto(Event @event)
        {
            if (@event == null) return null;
            EventDto @eventDto = new EventDto();
            eventDto.EventName = @event.EventName;
            return (@eventDto);
        }
    }
}
