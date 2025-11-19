using Application.DTOs;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Application.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorController : ControllerBase
    {
        private readonly IVendorService _vendorService;
        public VendorController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [ProducesResponseType(typeof(VendorDto), StatusCodes.Status200OK)]
        [HttpPost("AddVendor")]
        public async Task<IActionResult> AddVendor([FromBody] VendorDto vendorDTO)
        {
            if (!ModelState.IsValid)
                return Ok(vendorDTO);

            if (vendorDTO == null)
            {
                return BadRequest("fill panela");
            }
            var addedAdmin = await _vendorService.AddVendorAsync(vendorDTO);
            return Ok(addedAdmin);
        }

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [ProducesResponseType(typeof(VendorDto), StatusCodes.Status200OK)]
        [HttpDelete("DeleteVendor")]
        public async Task<IActionResult> DeleteVendor(Guid Id)
        {
            if (Id == null) { return BadRequest("id ela"); }
            await _vendorService.DeleteVendorAsync(Id);
            return Ok();
        }

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [HttpGet("1Vendor")]
        public async Task<IActionResult> GetVendorById(Guid vendorId)
        {
            var vendor = await _vendorService.GetVendorAsync(vendorId);
            if (vendor == null) return NotFound();
            return Ok(vendor);
        }
        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [HttpGet("AllVendor")]
        public async Task<IActionResult> GetAllAsync()
        {
            var vendors = await _vendorService.GetAllAsync();
            if (vendors == null) return NotFound();
            return Ok(vendors);
        }
    }
}
