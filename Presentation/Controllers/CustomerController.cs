using Application.DTOs;
using Application.Interface.IService;
using Application.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        [HttpPost("AddCustomer")]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerDto customerDTO)
        {
            if (!ModelState.IsValid)
                return Ok(customerDTO);

            if (customerDTO == null)
            {
                return BadRequest("fill panela");
            }
            var addedCustomer = await _customerService.AddCustomerAsync(customerDTO);
            return Ok(addedCustomer);
        }

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        [HttpDelete("DeleteCustomer")]
        public async Task<IActionResult> DeleteCustomer(Guid? Id)
        {

            if (Id == null)  return BadRequest("id ela"); 
            await _customerService.DeleteCustomerAsync(Id.Value);
            return Ok();
        }

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [HttpGet("1Customer")]
        public async Task<IActionResult> GetCustomerById(Guid customerId)
        {
            var customer = await _customerService.GetCustomerAsync(customerId);
            if (customer == null) return NotFound();
            return Ok(customer);
        }
        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [HttpGet("AllCustomer")]
        public async Task<IActionResult> GetAllAsync()
        {
            var customers = await _customerService.GetAllAsync();
            if (customers == null) return NotFound();
            return Ok(customers);
        }
    }
}
