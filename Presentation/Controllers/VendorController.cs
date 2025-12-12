using Application.Common; 
using Application.DTOs;
using Application.Interface.IService;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System; 
using System.Collections.Generic; 
using System.Linq; 

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorController : BaseApiController
    {
        private readonly IVendorService _vendorService;
        private readonly IPhotoService _photoService;
        public VendorController(IVendorService vendorService, IPhotoService photoService)
        {
            _vendorService = vendorService;
            _photoService = photoService;
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
                return Ok(ApiResponse<object?>.Success(null, "Vendor deleted successfully."));
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


        // --- 1. Vendor Logo Upload ---

        [HttpPost("UploadLogo/{vendorId}")]
        public async Task<IActionResult> UploadLogo(Guid vendorId, IFormFile file)
        {
            // 1. File இருக்கான்னு செக் பண்றோம்
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.Failure("No file uploaded."));

            // 2. Vendor இருக்காரான்னு செக் பண்றோம் (Optional but recommended)
            var existingVendor = await _vendorService.GetVendorAsync(vendorId);
            if (existingVendor == null)
                return NotFound(ApiResponse<object>.Failure("Vendor not found."));

            // 3. Cloudinary-ல் Upload பண்றோம் (PhotoService வழியாக)
            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null)
                return BadRequest(ApiResponse<object>.Failure(result.Error.Message));

            // 4. வந்த URL-ஐ Database-ல் Save பண்றோம்
            // குறிப்பு: இதற்காக Service Layer-ல் ஒரு method எழுத வேண்டும் (கீழே பாருங்கள்)
            var updateResult = await _vendorService.UpdateVendorLogoAsync(vendorId, result.SecureUrl.AbsoluteUri);

            if (!updateResult)
                return StatusCode(500, ApiResponse<object>.Failure("Failed to update logo in database."));

            return Ok(ApiResponse<object>.Success(new { Url = result.SecureUrl.AbsoluteUri }, "Logo uploaded successfully."));
        }


        // --- 2. Vendor Profile Photo Upload ---

        [HttpPost("UploadProfilePhoto/{vendorId}")]
        public async Task<IActionResult> UploadProfilePhoto(Guid vendorId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.Failure("No file uploaded."));

            var existingVendor = await _vendorService.GetVendorAsync(vendorId);
            if (existingVendor == null)
                return NotFound(ApiResponse<object>.Failure("Vendor not found."));

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null)
                return BadRequest(ApiResponse<object>.Failure(result.Error.Message));

            // URL-ஐ Database-ல் update செய்கிறோம்
            var updateResult = await _vendorService.UpdateVendorProfilePhotoAsync(vendorId, result.SecureUrl.AbsoluteUri);

            if (!updateResult)
                return StatusCode(500, ApiResponse<object>.Failure("Failed to update profile photo."));

            return Ok(ApiResponse<object>.Success(new { Url = result.SecureUrl.AbsoluteUri }, "Profile photo uploaded successfully."));
        }



    }
}