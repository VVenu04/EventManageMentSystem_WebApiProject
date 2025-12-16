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

        // ==========================================
        // 1. PUBLIC ENDPOINTS (Customers & Guests)
        // ==========================================

        // GET: api/Packages (Get All Active Packages)
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PackageDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPackages()
        {
            try
            {
                var packages = await _packageService.GetAllPackagesAsync();
                // Return empty list if null
                return Ok(ApiResponse<IEnumerable<PackageDto>>.Success(packages ?? new List<PackageDto>()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failure($"Internal Server Error: {ex.Message}"));
            }
        }

        // GET: api/Packages/{id} (Get Single Package Details)
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PackageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPackage(Guid id)
        {
            if (id == Guid.Empty) return BadRequest(ApiResponse<object>.Failure("Invalid Package ID."));

            try
            {
                var package = await _packageService.GetPackageByIdAsync(id);
                if (package == null)
                {
                    return NotFound(ApiResponse<object>.Failure("Package not found."));
                }
                return Ok(ApiResponse<PackageDto>.Success(package));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failure(ex.Message));
            }
        }

        // GET: api/Packages/vendor/{vendorId} (Get Vendor's Packages)
        [HttpGet("vendor/{vendorId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PackageDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPackagesByVendor(Guid vendorId)
        {
            if (vendorId == Guid.Empty) return BadRequest(ApiResponse<object>.Failure("Invalid Vendor ID."));

            try
            {
                var packages = await _packageService.GetPackagesByVendorIdAsync(vendorId);
                return Ok(ApiResponse<IEnumerable<PackageDto>>.Success(packages ?? new List<PackageDto>()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failure(ex.Message));
            }
        }

        // ==========================================
        // 2. VENDOR ENDPOINTS (Authenticated)
        // ==========================================

        // POST: api/Packages (Create Package Draft)
        [HttpPost]
        [Authorize(Roles = "Vendor")]
        [ProducesResponseType(typeof(ApiResponse<PackageDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreatePackage([FromBody] CreatePackageDto createPackageDto)
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid Token."));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.Failure("Validation Failed", errors));
            }

            try
            {
                // Pass CurrentUserId (Vendor ID) from Token
                var newPackage = await _packageService.CreatePackageAsync(createPackageDto, CurrentUserId);

                return CreatedAtAction(
                    nameof(GetPackage),
                    new { id = newPackage.PackageID },
                    ApiResponse<PackageDto>.Success(newPackage, "Package draft created successfully.")
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        // PUT: api/Packages/{id}/publish (Publish Package)
        [HttpPut("{packageId}/publish")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> PublishPackage(Guid packageId)
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid Token."));

            try
            {
                await _packageService.PublishPackageAsync(packageId, CurrentUserId);
                return Ok(ApiResponse<object>.Success(null, "Package published successfully."));
            }
            catch (Exception ex)
            {
                // Example: "Cannot publish: Waiting for partner approval."
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        // POST: api/Packages/add-services (Add Services to Package)
        [HttpPost("add-services")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> AddServices([FromBody] AddServicesToPackageDto dto)
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid Token."));

            try
            {
                await _packageService.AddServicesToPackageAsync(dto, CurrentUserId);
                return Ok(ApiResponse<object>.Success(null, "Services added to package successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        // ==========================================
        // 3. COLLABORATION ENDPOINTS
        // ==========================================

        // POST: api/Packages/invite (Invite another vendor)
        [HttpPost("invite")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> InviteVendor([FromBody] InviteVendorDto dto)
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid Token."));

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

        // PUT: api/Packages/request/{id}/respond?accept=true (Accept/Reject Invite)
        [HttpPut("request/{requestId}/respond")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> RespondToRequest(Guid requestId, [FromQuery] bool accept)
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid Token."));

            try
            {
                await _packageService.RespondToInvitationAsync(requestId, CurrentUserId, accept);
                return Ok(ApiResponse<object>.Success(null, accept ? "Invitation Accepted." : "Invitation Rejected."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }

        // GET: api/Packages/requests/{vendorId}
        [HttpGet("requests/{vendorId}")]
        [Authorize(Roles = "Vendor")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PackageRequestDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingRequests(Guid vendorId)
        {
            // Security Check
            if (CurrentUserId != vendorId)
            {
                return Unauthorized(ApiResponse<object>.Failure("Access Denied."));
            }

            try
            {
                var requests = await _packageService.GetPendingRequestsAsync(vendorId);

                return Ok(ApiResponse<IEnumerable<PackageRequestDto>>.Success(requests ?? new List<PackageRequestDto>()));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }
        [HttpGet("{packageId}/preview-for-collab")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetPackagePreview(Guid packageId)
        {
            try
            {
                // CurrentUserId = Login செய்த Vendor (Vendor B)
                var package = await _packageService.GetPackagePreviewForCollabAsync(packageId, CurrentUserId);
                return Ok(ApiResponse<PackageDto>.Success(package));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }


        // DELETE: api/Packages/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> DeletePackage(Guid id)
        {
            if (CurrentUserId == Guid.Empty) return Unauthorized(ApiResponse<object>.Failure("Invalid Token."));

            try
            {
                await _packageService.DeletePackageAsync(id, CurrentUserId);
                return Ok(ApiResponse<object>.Success(null, "Package deleted successfully and collaborators notified."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Failure(ex.Message));
            }
        }
    }
}