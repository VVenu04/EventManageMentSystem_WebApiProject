using Application.DTOs.Service;
using Application.DTOs.ServiceItem;
using Application.Interface.IService;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceItemController : ControllerBase
    {
        private readonly IServiceItemService _serviceService;

        public ServiceItemController(IServiceItemService serviceService)
        {
            _serviceService = serviceService;
        }

        // --- Vendor Protected Endpoint ---
        [HttpPost]
        [Authorize(Roles = "Vendor")] // Vendor மட்டும் தான் உருவாக்க முடியும்
        public async Task<ActionResult<ServiceItemDto>> CreateService(CreateServiceDto createServiceDto)
        {
            var vendorId = GetCurrentUserId();
            if (vendorId == Guid.Empty)
            {
                return Unauthorized("Invalid vendor token");
            }

            try
            {
                var newService = await _serviceService.CreateServiceAsync(createServiceDto, vendorId);
                return CreatedAtAction(nameof(GetService), new { id = newService.ServiceID }, newService);
            }
            catch (Exception ex)
            {
                // e.g., "Cannot add more than 5 photos"
                return BadRequest(ex.Message);
            }
        }

        // --- Public Endpoints (Customer-களுக்காக) ---

        // GET: api/services/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceItemDto>> GetService(Guid id)
        {
            var service = await _serviceService.GetServiceByIdAsync(id);
            if (service == null) return NotFound();
            return Ok(service);
        }

        // GET: api/services/vendor/{vendorId}
        [HttpGet("vendor/{vendorId}")]
        [AllowAnonymous] // ஒரு Vendor-இன் services-ஐப் பார்க்கலாம்
        public async Task<ActionResult<ServiceItemDto>> GetServicesByVendor(Guid vendorId)
        {
            var services = await _serviceService.GetServicesByVendorAsync(vendorId);
            return Ok(services);
        }

        // ... (Update/Delete endpoints இங்கே வரும்) ...

        // --- Helper Method ---
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            return Guid.Empty;
        }
        [HttpGet("search")]
        [AllowAnonymous] // யார் வேண்டுமானாலும் தேடலாம்
        public async Task<ActionResult<IEnumerable<ServiceItemDto>>> SearchServices([FromQuery] ServiceSearchDto searchDto)
        {
            // [FromQuery] பயன்படுத்துவதால் URL-ல் data வரும்.
            // எ.கா: api/services/search?searchTerm=wedding&minPrice=10000&eventDate=2025-12-20

            var services = await _serviceService.SearchServicesAsync(searchDto);
            return Ok(services);
        }

        // PUT: api/services/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Vendor")] // Vendor மட்டும் தான் எடிட் செய்ய முடியும்
        public async Task<IActionResult> UpdateService(Guid id, UpdateServiceDto updateServiceDto)
        {
            // 1. Token-ல் இருந்து Vendor ID-ஐ எடு
            var vendorId = GetCurrentUserId();
            if (vendorId == Guid.Empty) return Unauthorized("Invalid Token");

            try
            {
                // 2. Service-ஐ update செய்
                await _serviceService.UpdateServiceAsync(id, updateServiceDto, vendorId);
                return Ok(new { message = "Service updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Vendor")] // Vendor மட்டும் தான் அழிக்க முடியும்
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var vendorId = GetCurrentUserId(); // Token-ல் இருந்து ID எடு
            if (vendorId == Guid.Empty) return Unauthorized();

            try
            {
                await _serviceService.DeleteServiceAsync(id, vendorId);
                return Ok(new { message = "Service deleted successfully." });
            }
            catch (Exception ex)
            {
                // எ.கா: "Cannot delete because it is part of a Package"
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
