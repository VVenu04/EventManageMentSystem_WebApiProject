using Application.DTOs.Package;
using Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackagesController : ControllerBase
    {
        private readonly IPackageService _packageService;

        public PackagesController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpPost]
        [Authorize(Roles = "Vendor")] 
        public async Task<ActionResult<PackageDto>> CreatePackage(CreatePackageDto createPackageDto)
        {
            var vendorId = GetCurrentUserId();
            if (vendorId == Guid.Empty)
            {
                return Unauthorized("Invalid vendor token");
            }

            try
            {
                var newPackage = await _packageService.CreatePackageAsync(createPackageDto, vendorId);
                return CreatedAtAction(nameof(GetPackage), new { id = newPackage.PackageID }, newPackage);
            }
            catch (Exception ex)
            {
                // e.g., "Package price must be less than services"
                return BadRequest(ex.Message);
            }
        }


        // GET: api/packages/{id}
        [HttpGet("{id}")]
        [AllowAnonymous] 
        public async Task<ActionResult<PackageDto>> GetPackage(Guid id)
        {
            var package = await _packageService.GetPackageByIdAsync(id);
            if (package == null)
            {
                return NotFound();
            }
            return Ok(package);
        }

        // GET: api/packages/vendor/{vendorId}
        [HttpGet("vendor/{vendorId}")]
        [AllowAnonymous] 
        public async Task<ActionResult<IEnumerable<PackageDto>>> GetPackagesByVendor(Guid vendorId)
        {
            var packages = await _packageService.GetPackagesByVendorIdAsync(vendorId);
            return Ok(packages);
        }

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
        [HttpPost("invite")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> InviteVendor(InviteVendorDto dto)
        {
            var senderId = GetCurrentUserId();
            await _packageService.InviteVendorAsync(dto, senderId);
            return Ok("Invitation sent successfully.");
        }

        [HttpPut("request/{requestId}/respond")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> RespondToRequest(Guid requestId, [FromQuery] bool accept)
        {
            var vendorId = GetCurrentUserId();
            await _packageService.RespondToInvitationAsync(requestId, vendorId, accept);
            return Ok(accept ? "Invitation Accepted" : "Invitation Rejected");
        }

        [HttpPost("add-services")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> AddServices(AddServicesToPackageDto dto)
        {
            var vendorId = GetCurrentUserId();
            await _packageService.AddServicesToPackageAsync(dto, vendorId);
            return Ok("Services added successfully.");
        }

        [HttpPut("{packageId}/publish")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> PublishPackage(Guid packageId)
        {
            var vendorId = GetCurrentUserId();
            await _packageService.PublishPackageAsync(packageId, vendorId);
            return Ok("Package published to customers.");
        }
    }
}
