using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IRepo
{
    public interface IBookingRepository
    {
        Task<Booking> AddAsync(Booking booking);

        Task<Booking> GetByIdAsync(Guid bookingId);
    }
}
