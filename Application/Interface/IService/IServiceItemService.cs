using Application.DTOs.Service;
using Application.DTOs.ServiceItem;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface IServiceItemService
    {
        Task<ServiceItemDto> CreateServiceAsync(CreateServiceDto dto, List<IFormFile> images, Guid vendorId);
        Task UpdateServiceAsync(Guid serviceId, UpdateServiceDto dto, List<IFormFile> images, Guid vendorId);        
        Task DeleteServiceAsync(Guid serviceId, Guid vendorId);

        Task<ServiceItemDto> GetServiceByIdAsync(Guid serviceId);

        Task<IEnumerable<ServiceItemDto>> GetAllServicesAsync();

        Task<IEnumerable<ServiceItemDto>> GetServicesByVendorAsync(Guid vendorId);

        Task<IEnumerable<ServiceItemDto>> SearchServicesAsync(ServiceSearchDto searchDto);
        Task<bool> ToggleStatusAsync(Guid serviceId);
    }
}
