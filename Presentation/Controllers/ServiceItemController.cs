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
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/Services")]
    [ApiController]
    public class ServiceItemController : BaseApiController // 1. Inherit from BaseApiController
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
        public async Task<ActionResult<ServiceItemDto>> CreateService([FromForm] CreateServiceDto createServiceDto, [FromForm] List<IFormFile> images)
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid vendor token."));

            // Validation
            if (images == null || images.Count == 0)
                return BadRequest(ApiResponse<object>.Failure("At least one image is required."));

            if (images.Count > 5)
                return BadRequest(ApiResponse<object>.Failure("Maximum 5 images allowed."));

            try
            {
                // Service-க்கு DTO + Images இரண்டையும் அனுப்புகிறோம்
                var newService = await _serviceService.CreateServiceAsync(createServiceDto, images, CurrentUserId);

                return CreatedAtAction(
                    nameof(GetService),
                    new { id = newService.ServiceID },
                    ApiResponse<ServiceItemDto>.Success(newService, "Service created successfully.")
                );
            }
            catch (Exception ex)
            {
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

        [HttpGet("vendor/{vendorId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ServiceItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ServiceItemDto>>> GetServicesByVendor(Guid vendorId)
        {
            if (vendorId == Guid.Empty) return BadRequest(ApiResponse<object>.Failure("Invalid Vendor ID."));

            try
            {
                var services = await _serviceService.GetServicesByVendorAsync(vendorId);

                // 🚨 Success Response with Data (or Empty List)
                return Ok(ApiResponse<IEnumerable<ServiceItemDto>>.Success(services ?? new List<ServiceItemDto>()));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        //  GET: Search Services (Public) 
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ServiceItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ServiceItemDto>>> SearchServices([FromQuery] ServiceSearchDto searchDto)
        {
            try
            {
                // 🚨 FIX: DTO Null ஆக இருந்தால் Empty Object உருவாக்கு
                searchDto ??= new ServiceSearchDto();

                var services = await _serviceService.SearchServicesAsync(searchDto);

                // Service Layer-ல் ஏற்கனவே Mapping நடந்திருந்தால்:
                return Ok(ApiResponse<IEnumerable<ServiceItemDto>>.Success(services));

                // அல்லது இங்கே Map செய்வதாக இருந்தால்:
                // return Ok(ApiResponse<IEnumerable<ServiceItemDto>>.Success(services.Select(ServiceMapper.MapToServiceDto)));
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
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid Token"));

            if (id == Guid.Empty) return BadRequest(ApiResponse<object>.Failure("Invalid Service ID."));

            try
            {
                await _serviceService.UpdateServiceAsync(id, updateServiceDto, CurrentUserId);
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
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid Token"));

            try
            {
                await _serviceService.DeleteServiceAsync(id, CurrentUserId);
                return Ok(ApiResponse<object>.Success(null, "Service deleted successfully."));
            }
            catch (Exception ex)
            {
                // e.g., "Cannot delete because it is part of a Package"
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        
    }
}