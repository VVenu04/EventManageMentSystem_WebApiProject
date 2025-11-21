using Application.Common; 
using Application.DTOs;
using Application.Interface.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System; 
using System.Collections.Generic; 
using System.Linq; 

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

        //  AddVendor Method 

        [ProducesErrorResponseType(typeof(ApiResponse<VendorDto>))]
        [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status201Created)] 
        [HttpPost("AddVendor")]
        public async Task<IActionResult> AddVendor([FromBody] VendorDto vendorDTO)
        {
            //  Validation Check (ModelState)
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<VendorDto>.Failure("Validation failed.", validationErrors));
            }

            //  Null Check 
            if (vendorDTO == null)
            {
                return BadRequest(ApiResponse<VendorDto>.Failure("Vendor data cannot be null."));
            }

            try
            {
                var addedVendor = await _vendorService.AddVendorAsync(vendorDTO);

                //  Success Response (201 Created)
                return CreatedAtAction(
                    nameof(GetVendorById),
                    new { vendorId = addedVendor.VendorID }, // NOTE: Requires VendorID property in VendorDto
                    ApiResponse<VendorDto>.Success(addedVendor, "Vendor registered successfully.")
                );
            }
            catch (Exception ex)
            {
                // Catches errors like "Email already exists"
                return StatusCode(500, ApiResponse<VendorDto>.Failure(ex.Message));
            }
        }

        // --- DeleteVendor Method ---

        [ProducesErrorResponseType(typeof(ApiResponse<object>))]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [HttpDelete("DeleteVendor")]
        public async Task<IActionResult> DeleteVendor(Guid Id)
        {
            // 4. Fixed Guid Check (Replaces "id ela")
            if (Id == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Failure("A valid Vendor ID is required."));
            }

            try
            {
                await _vendorService.DeleteVendorAsync(Id);
                // 5. Success Response
                return Ok(ApiResponse<object>.Success(null, "Vendor deleted successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failure(ex.Message));
            }
        }

        // --- GetVendorById Method ---

        [ProducesErrorResponseType(typeof(ApiResponse<VendorDto>))]
        [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status200OK)]
        [HttpGet("1Vendor")]
        public async Task<IActionResult> GetVendorById(Guid vendorId)
        {
            if (vendorId == Guid.Empty)
            {
                return BadRequest(ApiResponse<VendorDto>.Failure("A valid Vendor ID is required."));
            }

            var vendor = await _vendorService.GetVendorAsync(vendorId);

            // 6. Not Found Check
            if (vendor == null)
            {
                return NotFound(ApiResponse<VendorDto>.Failure("Vendor not found."));
            }

            // 7. Success Response
            return Ok(ApiResponse<VendorDto>.Success(vendor));
        }

        // --- GetAllAsync Method ---

        [ProducesErrorResponseType(typeof(ApiResponse<IEnumerable<VendorDto>>))]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VendorDto>>), StatusCodes.Status200OK)]
        [HttpGet("AllVendor")]
        public async Task<IActionResult> GetAllAsync()
        {
            var vendors = await _vendorService.GetAllAsync();

            // 8. Empty List Handling
            if (vendors == null || !vendors.Any())
            {
                return Ok(ApiResponse<IEnumerable<VendorDto>>.Success(new List<VendorDto>(), "No vendors found."));
            }

            // 9. Success Response
            return Ok(ApiResponse<IEnumerable<VendorDto>>.Success(vendors));
        }
    }
}