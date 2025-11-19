using Application.Interface.IRepo;
using Domain.Entities;
using infrastucure.Data;
using infrastucure.GenericRepositary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Repositary
{
    public class FunctionRepository : GenericRepo<Domain.Entities.ServiceItem>, IFunctionRepo
    {
        public FunctionRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task UpdateAsync(ServiceItem service)
        {
            _dbContext.ServiceItems.Update(service);
            await _dbContext.SaveChangesAsync();
        }
    }
}
