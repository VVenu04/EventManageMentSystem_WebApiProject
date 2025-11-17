using Application.DTOs.Service;
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
    }
}
