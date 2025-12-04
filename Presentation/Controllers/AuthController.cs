using Application.Common;
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


        [HttpPost("customer/google-login")]
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

            // 1. Update the profile
            var success = await _authService.UpdateCustomerProfileAsync(CurrentUserId, dto);

            if (!success) return BadRequest(ApiResponse<object>.Failure("Failed to update profile."));

            // 2. 🚨 FIX: Fetch the updated user to return to Frontend
            // (இதற்கு AuthService-ல் GetUserById அல்லது AuthRepo-ஐ பயன்படுத்தலாம்)
            // இங்கு எளிமைக்காக DTO-வில் இருந்தே அனுப்புகிறோம், ஆனால் ரியல்-டைமில் DB-ல் இருந்து எடுப்பது நல்லது.

            var updatedData = new
            {
                displayName = dto.Name,       // Frontend 'displayName' எதிர்பார்க்கிறது
                phoneNumber = dto.PhoneNumber,
                location = dto.Location,
                img = dto.ProfilePhotoUrl
            };

            // 3. Return the updated data inside Success
            return Ok(ApiResponse<object>.Success(updatedData, "Profile updated successfully."));
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