using Application.Common;
using Application.DTOs.Admin;
using Application.Interface.IService;
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
    public class AdminController : BaseApiController
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminServic)
        {
            _adminService = adminServic;
        }
        [HttpGet("overview")]
        [Authorize(Roles = "Admin")] // Admin Only
        public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> GetDashboardOverview()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return Ok(ApiResponse<AdminDashboardDto>.Success(stats));
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
        [HttpGet("transactions")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionDto>>>> GetAllTransactions()
        {
            var transactions = await _adminService.GetAllTransactionsAsync();
            return Ok(ApiResponse<IEnumerable<TransactionDto>>.Success(transactions));
        }
















        //site settings


            // ... Constructor ...

            // 1. GET SETTINGS (Public - for Footer)
            [HttpGet("settings")]
            [AllowAnonymous]
            public async Task<IActionResult> GetSettings()
            {
                var settings = await _adminService.GetSystemSettingsAsync();
                return Ok(ApiResponse<SystemSettingsDto>.Success(settings));
            }

            // 2. UPDATE SETTINGS (Admin Only)
            [HttpPut("settings")]
            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> UpdateSettings([FromBody] SystemSettingsDto dto)
            {
                var updated = await _adminService.UpdateSystemSettingsAsync(dto);
                return Ok(ApiResponse<SystemSettingsDto>.Success(updated, "Settings updated successfully."));
            }

            // 3. CHANGE PASSWORD
            [HttpPost("change-password")]
            [Authorize]
            public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
            {
                var result = await _adminService.ChangePasswordAsync(CurrentUserId, dto);
                if (!result) return BadRequest(ApiResponse<object>.Failure("Incorrect current password."));

                return Ok(ApiResponse<object>.Success(null, "Password changed successfully."));
            }
        }

    }
