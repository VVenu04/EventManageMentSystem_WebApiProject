using Application.DTOs.Auth;
using Application.DTOs.Forgot;
using Application.Interface.IAuth;

// Interface namespace-ஐ உறுதிப்படுத்தவும் (Application.Interface.IService அல்லது Application.Interfaces)
using Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims; // <-- 1. இந்த namespace மிக முக்கியம்
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // --- Customer Routes ---
        [HttpPost("customer/register")]
        public async Task<ActionResult<AuthResponseDto>> RegisterCustomer(RegisterCustomerDto dto)
        {
            var result = await _authService.RegisterCustomerAsync(dto);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpPost("customer/login")]
        public async Task<ActionResult<AuthResponseDto>> LoginCustomer(LoginDto dto)
        {
            var result = await _authService.LoginCustomerAsync(dto);
            if (!result.IsSuccess) return Unauthorized(result.Message);
            return Ok(result);
        }

        // --- Vendor Routes ---
        [HttpPost("vendor/register")]
        public async Task<ActionResult<AuthResponseDto>> RegisterVendor(RegisterVendorDto dto)
        {
            var result = await _authService.RegisterVendorAsync(dto);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpPost("vendor/login")]
        public async Task<ActionResult<AuthResponseDto>> LoginVendor(LoginDto dto)
        {
            var result = await _authService.LoginVendorAsync(dto);
            if (!result.IsSuccess) return Unauthorized(result.Message);
            return Ok(result);
        }

        // --- Admin Route ---
        [HttpPost("admin/login")]
        public async Task<ActionResult<AuthResponseDto>> LoginAdmin(LoginDto dto)
        {
            var result = await _authService.LoginAdminAsync(dto);
            if (!result.IsSuccess) return Unauthorized(result.Message);
            return Ok(result);
        }

        // --- Profile Update Routes ---

        [HttpPut("vendor/profile")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> UpdateVendorProfile(UpdateVendorProfileDto dto)
        {
            // 2. Helper method-ஐப் பயன்படுத்தி ID-ஐப் பெறுதல்
            var vendorId = GetCurrentUserId();

            if (vendorId == Guid.Empty) return Unauthorized();

            var success = await _authService.UpdateVendorProfileAsync(vendorId, dto);

            if (!success) return BadRequest("Failed to update profile.");
            return Ok("Profile updated successfully.");
        }

        [HttpPut("customer/profile")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateCustomerProfile(UpdateCustomerProfileDto dto)
        {
            var customerId = GetCurrentUserId();

            if (customerId == Guid.Empty) return Unauthorized();

            var success = await _authService.UpdateCustomerProfileAsync(customerId, dto);

            if (!success) return BadRequest("Failed to update profile.");
            return Ok("Profile updated successfully.");
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto
                {
                    Success = false,
                    Message = "Invalid request data",
                    Data = ModelState
                });
            }

            var result = await _authService.ForgotPasswordAsync(dto);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Verify OTP (Optional step before password reset)
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto
                {
                    Success = false,
                    Message = "Invalid request data",
                    Data = ModelState
                });
            }

            var result = await _authService.VerifyOtpAsync(dto);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Reset password using OTP
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDto
                {
                    Success = false,
                    Message = "Invalid request data",
                    Data = ModelState
                });
            }

            var result = await _authService.ResetPasswordAsync(dto);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
    

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