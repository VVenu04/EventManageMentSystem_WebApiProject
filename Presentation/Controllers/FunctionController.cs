using Application.DTOs;
using Application.Interface.IService;
using Application.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FunctionController : ControllerBase
    {
        private readonly IFunctionService _functionService;
        public FunctionController(IFunctionService functionService)
        {
            _functionService = functionService;
        }

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [ProducesResponseType(typeof(FunctionDto), StatusCodes.Status200OK)]
        [HttpPost("AddFunction")]
        public async Task<IActionResult> AddFunction([FromBody] FunctionDto functionDTO)
        {
            if (!ModelState.IsValid)
                return Ok(functionDTO);

            if (functionDTO == null)
            {
                return BadRequest("fill panela");
            }
            var addedFunction = await _functionService.AddFunctionAsync(functionDTO);
            return Ok(addedFunction);
        }

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [ProducesResponseType(typeof(FunctionDto), StatusCodes.Status200OK)]
        [HttpDelete("DeleteFunction")]
        public async Task<IActionResult> DeleteFunction(Guid Id)
        {
            if (Id == null) { return BadRequest("id ela"); }
            await _functionService.DeleteFunctionAsync(Id);
            return Ok();
        }

        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [HttpGet("1Function")]
        public async Task<IActionResult> GetFunctionById(Guid functionId)
        {
            var function = await _functionService.GetFunctionAsync(functionId);
            if (function == null) return NotFound();
            return Ok(function);
        }
        [ProducesErrorResponseType(typeof(BadRequestResult))]
        [HttpGet("AllFunction")]
        public async Task<IActionResult> GetAllAsync()
        {
            var functions = await _functionService.GetAllAsync();
            if (functions == null) return NotFound();
            return Ok(functions);
        }
    }
}
