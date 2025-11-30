using Application.Common;
using Application.DTOs.Package;
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
    [Route("api/[controller]")]
    [ApiController]
    public class PackagesController : BaseApiController
    {
        private readonly IPackageService _packageService;

        public PackagesController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        // POST: CreatePackage 
        [HttpPost]
        [Authorize(Roles = "Vendor")]
        [ProducesResponseType(typeof(ApiResponse<PackageDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePackage([FromBody] CreatePackageDto createPackageDto)
        {
            // Change 1: Use CurrentUserId property directly
            if (CurrentUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Failure("Authentication failed: Invalid vendor token."));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.Failure("Validation failed.", errors));
            }

            try
            {
                // Change 2: Pass CurrentUserId
                var newPackage = await _packageService.CreatePackageAsync(createPackageDto, CurrentUserId);

                return CreatedAtAction(
                    nameof(GetPackage),
                    new { id = newPackage.PackageID },
                    ApiResponse<PackageDto>.Success(newPackage, "Package draft created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        // GET: GetPackage 
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PackageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPackage(Guid id)
        {
            if (id == Guid.Empty) return BadRequest(ApiResponse<object>.Failure("Invalid ID."));

            var package = await _packageService.GetPackageByIdAsync(id);

            if (package == null)
            {
                return NotFound(ApiResponse<object>.Failure($"Package with ID {id} not found."));
            }
            return Ok(ApiResponse<PackageDto>.Success(package));
        }

        // GET: GetPackagesByVendor 
        [HttpGet("vendor/{vendorId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PackageDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPackagesByVendor(Guid vendorId)
        {
            if (vendorId == Guid.Empty) return BadRequest(ApiResponse<object>.Failure("Invalid Vendor ID."));

            var packages = await _packageService.GetPackagesByVendorIdAsync(vendorId);

            // Return success even if list is empty
            return Ok(ApiResponse<IEnumerable<PackageDto>>.Success(packages ?? new List<PackageDto>()));
        }

        // POST: InviteVendor 
        [HttpPost("invite")]
        [Authorize(Roles = "Vendor")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> InviteVendor([FromBody] InviteVendorDto dto)
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid token."));

            if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Failure("Validation failed.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            try
            {
                await _packageService.InviteVendorAsync(dto, CurrentUserId);
                return Ok(ApiResponse<object>.Success(null, "Invitation sent successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        // PUT: RespondToRequest 
        [HttpPut("request/{requestId}/respond")]
        [Authorize(Roles = "Vendor")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RespondToRequest(Guid requestId, [FromQuery] bool accept)
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid token."));

            try
            {
                await _packageService.RespondToInvitationAsync(requestId, CurrentUserId, accept);
                return Ok(ApiResponse<object>.Success(null, accept ? "Invitation Accepted" : "Invitation Rejected"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        // POST: AddServices 
        [HttpPost("add-services")]
        [Authorize(Roles = "Vendor")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddServices([FromBody] AddServicesToPackageDto dto)
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid token."));

            try
            {
                await _packageService.AddServicesToPackageAsync(dto, CurrentUserId);
                return Ok(ApiResponse<object>.Success(null, "Services added successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        // PUT: PublishPackage 
        [HttpPut("{packageId}/publish")]
        [Authorize(Roles = "Vendor")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PublishPackage(Guid packageId)
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid token."));

            try
            {
                await _packageService.PublishPackageAsync(packageId, CurrentUserId);
                return Ok(ApiResponse<object>.Success(null, "Package published to customers."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        
    }
}