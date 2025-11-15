using Application.DTOs.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface IServiceService
    {
        Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto, Guid vendorId);

        // Vendor-ஆல் ஒரு Service-ஐ update செய்ய
        Task UpdateServiceAsync(Guid serviceId, CreateServiceDto updateServiceDto, Guid vendorId);

        // Vendor-ஆல் ஒரு Service-ஐ delete செய்ய
        Task DeleteServiceAsync(Guid serviceId, Guid vendorId);

        // Customer/User-ஆல் ஒரு Service-ஐப் பார்க்க
        Task<ServiceDto> GetServiceByIdAsync(Guid serviceId);

        // Customer/User-ஆல் எல்லா Services-ஐயும் பார்க்க
        Task<IEnumerable<ServiceDto>> GetAllServicesAsync();

        // Customer/User-ஆல் ஒரு Vendor-இன் Services-ஐப் பார்க்க
        Task<IEnumerable<ServiceDto>> GetServicesByVendorAsync(Guid vendorId);
    }
}
