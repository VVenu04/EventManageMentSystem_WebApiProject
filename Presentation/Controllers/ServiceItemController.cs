using Application.Common; 
using Application.DTOs.Service;
using Application.DTOs.ServiceItem; 
using Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Presentation.Controllers
{
    
    [Route("api/Services")]
    [ApiController]
    public class ServiceItemController : ControllerBase
    {
        private readonly IServiceItemService _serviceService;

        public ServiceItemController(IServiceItemService serviceService)
        {
            _serviceService = serviceService;
        }

        // POST: Create Service (Vendor Only) 
        [HttpPost]
        [Authorize(Roles = "Vendor")]
        [ProducesResponseType(typeof(ApiResponse<ServiceItemDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ServiceItemDto>> CreateService([FromBody] CreateServiceDto createServiceDto)
        {
            var vendorId = GetCurrentUserId();
            if (vendorId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Invalid vendor token."));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ServiceItemDto>.Failure("Validation failed.", errors));
            }

            try
            {
                var newService = await _serviceService.CreateServiceAsync(createServiceDto, vendorId);

                return CreatedAtAction(
                    nameof(GetService),
                    new { id = newService.ServiceID },
                    ApiResponse<ServiceItemDto>.Success(newService, "Service created successfully.")
                );
            }
            catch (Exception ex)
            {
                // Example. "Cannot add more than 5 photos"
                return BadRequest(ApiResponse<ServiceItemDto>.Failure(ex.Message));
            }
        }

        //  GET: Get Service By ID (Public) 
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ServiceItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ServiceItemDto>> GetService(Guid id)
        {
            if (id == Guid.Empty) return BadRequest(ApiResponse<ServiceItemDto>.Failure("Invalid ID."));

            var service = await _serviceService.GetServiceByIdAsync(id);

            if (service == null)
            {
                return NotFound(ApiResponse<ServiceItemDto>.Failure("Service not found."));
            }

            return Ok(ApiResponse<ServiceItemDto>.Success(service));
        }

        //  GET: Get Services By Vendor (Public)
        [HttpGet("vendor/{vendorId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ServiceItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ServiceItemDto>> GetServicesByVendor(Guid vendorId)
        {
            if (vendorId == Guid.Empty) return BadRequest(ApiResponse<object>.Failure("Invalid Vendor ID."));

            var services = await _serviceService.GetServicesByVendorAsync(vendorId);

            // Return empty list instead of 404 if vendor has no services
            return Ok(ApiResponse<IEnumerable<ServiceItemDto>>.Success(services ?? new List<ServiceItemDto>()));
        }

        //  GET: Search Services (Public) 
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ServiceItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ServiceItemDto>>> SearchServices([FromQuery] ServiceSearchDto searchDto)
        {
            try
            {
                var services = await _serviceService.SearchServicesAsync(searchDto);
                return Ok(ApiResponse<IEnumerable<ServiceItemDto>>.Success(services ?? new List<ServiceItemDto>()));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        //  PUT: Update Service (Vendor Only) 
        [HttpPut("{id}")]
        [Authorize(Roles = "Vendor")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpdateServiceDto updateServiceDto)
        {
            var vendorId = GetCurrentUserId();
            if (vendorId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid Token"));

            if (id == Guid.Empty) return BadRequest(ApiResponse<object>.Failure("Invalid Service ID."));

            try
            {
                await _serviceService.UpdateServiceAsync(id, updateServiceDto, vendorId);
                return Ok(ApiResponse<object>.Success(null, "Service updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        //  DELETE: Delete Service (Vendor Only) 
        [HttpDelete("{id}")]
        [Authorize(Roles = "Vendor")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var vendorId = GetCurrentUserId();
            if (vendorId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid Token"));

            try
            {
                await _serviceService.DeleteServiceAsync(id, vendorId);
                return Ok(ApiResponse<object>.Success(null, "Service deleted successfully."));
            }
            catch (Exception ex)
            {
                // e.g., "Cannot delete because it is part of a Package"
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        //  Helper Method 
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