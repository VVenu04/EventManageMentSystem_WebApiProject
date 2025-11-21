using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services;
using Domain.Entities;

namespace Application.Interface.IRepo
{
    public interface IServiceItemRepository
    {
        Task<ServiceItem> AddAsync(ServiceItem service);
        Task UpdateAsync(ServiceItem service);
        Task DeleteAsync(ServiceItem service);

        Task<ServiceItem?> GetByIdAsync(Guid serviceId);
        Task<IEnumerable<ServiceItem>> GetAllAsync();

        Task<IEnumerable<ServiceItem>> GetByVendorIdAsync(Guid vendorId);
        Task<bool> IsServiceInAnyPackageAsync(Guid serviceId);

    }
}
