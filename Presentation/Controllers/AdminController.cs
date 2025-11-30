using Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Application.DTOs;
using Application.Interface.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminServic)
        {
            _adminService = adminServic;
        }

 
        [ProducesErrorResponseType(typeof(ApiResponse<AdminDto>))]
        [ProducesResponseType(typeof(ApiResponse<AdminDto>), StatusCodes.Status201Created)] 
        [HttpPost("AddAdmin")]
        public async Task<IActionResult> AddAdmin([FromBody] AdminDto adminDTO)
        {
            // Invalid ModelState check 
            if (!ModelState.IsValid)
            {
                // Get all validation error messages
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<AdminDto>.Failure("Validation failed.", validationErrors));
            }

            if (adminDTO == null)
            {
                return BadRequest(ApiResponse<AdminDto>.Failure("Admin data cannot be null."));
            }

            try
            {
                var addedAdmin = await _adminService.AddAdminAsync(adminDTO);

                return CreatedAtAction(
                    nameof(GetAdminById),
                    new { adminId = addedAdmin.AdminID },
                    ApiResponse<AdminDto>.Success(addedAdmin, "Admin created successfully.")
                );
            }
            catch (Exception ex)
            {
                //  catch any errors from the service (like password hashing)
                return StatusCode(500, ApiResponse<AdminDto>.Failure(ex.Message));
            }
        }

 
        [ProducesErrorResponseType(typeof(ApiResponse<object>))]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [HttpDelete("DeleteAdmin")]
        public async Task<IActionResult> DeleteAdmin(Guid? Id)
        {
   
            if (Id == null || Id.Value == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Failure("A valid Admin ID is required."));
            }

            try
            {
                await _adminService.DeleteAdminAsync(Id);
      
                return Ok(ApiResponse<object?>.Success(null, "Admin deleted successfully."));
            }
            catch (Exception ex)
            {
                // catch any errors from the service
                return StatusCode(500, ApiResponse<object>.Failure(ex.Message));
            }
        }


        [ProducesErrorResponseType(typeof(ApiResponse<AdminDto>))]
        [ProducesResponseType(typeof(ApiResponse<AdminDto>), StatusCodes.Status200OK)]
        [HttpGet("1Admin")]
        public async Task<IActionResult> GetAdminById(Guid adminId)
        {
            //  Validation for the ID 
            if (adminId == Guid.Empty)
            {
                return BadRequest(ApiResponse<AdminDto>.Failure("A valid Admin ID is required."));
            }

            var admin = await _adminService.GetAdminAsync(adminId);

            if (admin == null)
            {
                return NotFound(ApiResponse<AdminDto>.Failure("Admin not found."));
            }

            return Ok(ApiResponse<AdminDto>.Success(admin));
        }


        [ProducesErrorResponseType(typeof(ApiResponse<IEnumerable<AdminDto>>))]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AdminDto>>), StatusCodes.Status200OK)]
        [HttpGet("AllAdmin")]
        public async Task<IActionResult> GetAllAsync()
        {
            var admins = await _adminService.GetAllAsync();

            //  Handling of 'null' or 'empty' list 
            if (admins == null || !admins.Any())
            {
                return Ok(ApiResponse<IEnumerable<AdminDto>>.Success(new List<AdminDto>(), "No admins found."));
            }

            return Ok(ApiResponse<IEnumerable<AdminDto>>.Success(admins));
        }
    }
}