using Application.DTOs.Service;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapper
{
    public class ServiceMapper
    {
        public static ServiceItemDto MapToServiceDto(ServiceItem service)
        {
            if (service == null) return null;

            return new ServiceItemDto
            {
                ServiceID = service.ServiceItemID,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                Location = service.Location,
                Active = service.Active,
                EventPerDayLimit = service.EventPerDayLimit,
                TimeLimit = service.TimeLimit,
                VendorID = service.VendorID,
                VendorName = service.Vendor?.Name,
                CategoryID = (Guid)(service.CategoryID == null ? Guid.Empty : service.CategoryID),
                CategoryName = service.Category?.CategoryName ?? "Unknown Category",

                // --- Events Mapping (New Change) ---
                EventNames = service.Events?
                                .Select(e => e.EventName)
                                .ToList() ?? new List<string>(),

                // --- Photos Mapping ---
                ImageUrls = service.ServiceImages?
                                .Select(img => img.ImageUrl)
                                .ToList() ?? new List<string>()
            };
        }
    }
}
