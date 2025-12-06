using Application.Interface.IRepo;
using Domain.Entities;
using infrastructure.GenericRepositary;
using infrastucure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Repositary
{
    public class EventRepository : GenericRepo<Event>, IEventRepo
    {
        public EventRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task UpdateAsync(Event @event)
        {
            _dbContext.Events.Update(@event);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<Event?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Events.FindAsync(id);
        }
    }
}
