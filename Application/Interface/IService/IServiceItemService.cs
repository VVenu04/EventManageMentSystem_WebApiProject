using Application.DTOs.Service;
using Application.DTOs.ServiceItem;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface IServiceItemService
    {
        Task<ServiceItemDto> CreateServiceAsync(CreateServiceDto dto, Guid vendorId);

        // 🚨 'CreateServiceDto'-வை 'UpdateServiceDto'-ஆக மாற்றவும்
        Task UpdateServiceAsync(Guid serviceId, UpdateServiceDto updateServiceDto, Guid vendorId);
        // Vendor-ஆல் ஒரு Service-ஐ delete செய்ய
        Task DeleteServiceAsync(Guid serviceId, Guid vendorId);

        // Customer/User-ஆல் ஒரு Service-ஐப் பார்க்க
        Task<ServiceItemDto> GetServiceByIdAsync(Guid serviceId);

        // Customer/User-ஆல் எல்லா Services-ஐயும் பார்க்க
        Task<IEnumerable<ServiceItemDto>> GetAllServicesAsync();

        // Customer/User-ஆல் ஒரு Vendor-இன் Services-ஐப் பார்க்க
        Task<IEnumerable<ServiceItemDto>> GetServicesByVendorAsync(Guid vendorId);

              // FIXED: Changed the return type from ServiceItem to ServiceItemDto (28th Nov)
        Task<IEnumerable<ServiceItemDto>> SearchServicesAsync(ServiceSearchDto searchDto);

    }
}
