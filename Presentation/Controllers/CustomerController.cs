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
    public class CustomerController : BaseApiController
    {
        private readonly ICustomerService _customerService;
        private readonly IPhotoService _photoService;
     
        public CustomerController(ICustomerService customerService, IPhotoService photoService)
        {
            _customerService = customerService;
            _photoService = photoService;
        }

        // AddCustomer Method 

        [ProducesErrorResponseType(typeof(ApiResponse<CustomerDto>))]
        [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status201Created)] 
        [HttpPost("AddCustomer")]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerDto customerDTO)
        {
            //  Validation Check
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<CustomerDto>.Failure("Validation failed.", validationErrors));
            }

            //  Null Check
            if (customerDTO == null)
            {
                return BadRequest(ApiResponse<CustomerDto>.Failure("Customer data cannot be null."));
            }

            try
            {
                var addedCustomer = await _customerService.AddCustomerAsync(customerDTO);

                //  Success Response (201 Created)
                return CreatedAtAction(
                    nameof(GetCustomerById),
                    new { customerId = addedCustomer.CustomerID },
                    ApiResponse<CustomerDto>.Success(addedCustomer, "Customer created successfully.")
                );
            }
            catch (Exception ex)
            {
                // Catches errors (Example - "Email already exists" or DB connection issues)
                return StatusCode(500, ApiResponse<CustomerDto>.Failure(ex.Message));
            }
        }

        //  DeleteCustomer Method 

        [ProducesErrorResponseType(typeof(ApiResponse<object>))]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [HttpDelete("DeleteCustomer")]
        public async Task<IActionResult> DeleteCustomer(Guid Id)
        {
            //  Guid Empty Check
            if (Id == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Failure("A valid Customer ID is required."));
            }

            try
            {
                await _customerService.DeleteCustomerAsync(Id);
                return Ok(ApiResponse<object?>.Success(null, "Customer deleted successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failure(ex.Message));
            }
        }

        //  GetCustomerById Method 

        [ProducesErrorResponseType(typeof(ApiResponse<CustomerDto>))]
        [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
        [HttpGet("1Customer")]
        public async Task<IActionResult> GetCustomerById(Guid customerId)
        {
            if (customerId == Guid.Empty)
            {
                return BadRequest(ApiResponse<CustomerDto>.Failure("A valid Customer ID is required."));
            }

            var customer = await _customerService.GetCustomerAsync(customerId);

            //  Not Found Check
            if (customer == null)
            {
                return NotFound(ApiResponse<CustomerDto>.Failure("Customer not found."));
            }

            return Ok(ApiResponse<CustomerDto>.Success(customer));
        }

        //  GetAllAsync Method 

        [ProducesErrorResponseType(typeof(ApiResponse<IEnumerable<CustomerDto>>))]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerDto>>), StatusCodes.Status200OK)]
        [HttpGet("AllCustomer")]
        public async Task<IActionResult> GetAllAsync()
        {
            var customers = await _customerService.GetAllAsync();

            //  Empty List Handling
            if (customers == null || !customers.Any())
            {
                return Ok(ApiResponse<IEnumerable<CustomerDto>>.Success(new List<CustomerDto>(), "No customers found."));
            }

            return Ok(ApiResponse<IEnumerable<CustomerDto>>.Success(customers));
        }



        [HttpPost("UploadProfilePhoto/{customerId}")]
        public async Task<IActionResult> UploadProfilePhoto(Guid customerId, IFormFile file)
        {
            // File இருக்கான்னு செக் பண்றோம்
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.Failure("No file uploaded."));

            // Customer இருக்காரான்னு செக் பண்றோம்
            var existingCustomer = await _customerService.GetCustomerAsync(customerId); // அல்லது உங்கள் Get Method
            if (existingCustomer == null)
                return NotFound(ApiResponse<object>.Failure("Customer not found."));

            // Cloudinary-ல் Upload பண்றோம்
            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null)
                return BadRequest(ApiResponse<object>.Failure(result.Error.Message));

            // Database-ல் URL-ஐ update பண்றோம்
            var updateResult = await _customerService.UpdateCustomerProfilePhotoAsync(customerId, result.SecureUrl.AbsoluteUri);

            if (!updateResult)
                return StatusCode(500, ApiResponse<object>.Failure("Failed to update profile photo in database."));

            return Ok(ApiResponse<object>.Success(new { Url = result.SecureUrl.AbsoluteUri }, "Profile photo uploaded successfully."));
        }

        

 
    }
}