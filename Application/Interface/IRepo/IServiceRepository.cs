using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services;
using Domain.Entities;

namespace Application.Interface.IRepo
{
    public interface IServiceRepository
    {
        Task<Service> AddAsync(Service service);
        Task UpdateAsync(Service service);
        Task DeleteAsync(Service service);

        Task<Service> GetByIdAsync(Guid serviceId);
        Task<IEnumerable<Service>> GetAllAsync();

        Task<IEnumerable<Service>> GetByVendorIdAsync(Guid vendorId);
    }
}
