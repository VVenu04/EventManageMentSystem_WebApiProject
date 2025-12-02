using Application.DTOs.Auth;
using Application.DTOs.Forgot;
using Application.DTOs.Google;
using Application.Interface.IAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 1. இங்கே 'ControllerBase'-க்கு பதில் 'BaseApiController' பயன்படுத்தவும்
    public class AuthController : BaseApiController
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
        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<IActionResult> SignInWithGoogle([FromBody] GoogleAuthRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.IdToken))
                return BadRequest("IdToken is required.");

            try
            {
               var result = await _authService.SignInWithGoogleAsync(dto.IdToken);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // For debugging - in production, log properly
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
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
            if (CurrentUserId == Guid.Empty) return Unauthorized();

            var success = await _authService.UpdateVendorProfileAsync(CurrentUserId, dto);

            if (!success) return BadRequest("Failed to update profile.");
            return Ok("Profile updated successfully.");
        }

        [HttpPut("customer/profile")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateCustomerProfile(UpdateCustomerProfileDto dto)
        {
            
            if (CurrentUserId == Guid.Empty) return Unauthorized();

            var success = await _authService.UpdateCustomerProfileAsync(CurrentUserId, dto);

            if (!success) return BadRequest("Failed to update profile.");
            return Ok("Profile updated successfully.");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto { Success = false, Message = "Invalid request data", Data = ModelState });

            var result = await _authService.ForgotPasswordAsync(dto);

            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto { Success = false, Message = "Invalid request data", Data = ModelState });

            var result = await _authService.VerifyOtpAsync(dto);

            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto { Success = false, Message = "Invalid request data", Data = ModelState });

            var result = await _authService.ResetPasswordAsync(dto);

            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        
    }
}