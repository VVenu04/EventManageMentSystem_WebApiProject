using Application.Common;
using Application.DTOs;
using Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // --- AddCategory ---
        [ProducesErrorResponseType(typeof(ApiResponse<CategoryDto>))]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
        [HttpPost("AddCategory")]
        public async Task<IActionResult> AddCategory([FromBody] CategoryDto categoryDTO)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<CategoryDto>.Failure("Validation failed.", validationErrors));
            }

            if (categoryDTO == null)
            {
                return BadRequest(ApiResponse<CategoryDto>.Failure("Category data cannot be null."));
            }

            try
            {
                var addedCategory = await _categoryService.AddCategoryAsync(categoryDTO);

                return CreatedAtAction(
                    nameof(GetCategoryById),
                    new { categoryId = addedCategory.CategoryID },
                    ApiResponse<CategoryDto>.Success(addedCategory, "Category created successfully.")
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.Failure(ex.Message));
            }
        }

        // --- DeleteCategory ---
        [ProducesErrorResponseType(typeof(ApiResponse<object>))]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [HttpDelete("DeleteCategory")]
        public async Task<IActionResult> DeleteCategory(Guid Id)
        {
            if (Id == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Failure("A valid Category ID is required."));
            }

            try
            {
                await _categoryService.DeleteCategoryAsync(Id);
                return Ok(ApiResponse<object?>.Success(null, "Category deleted successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failure(ex.Message));
            }
        }

        // --- GetCategoryById ---
        [ProducesErrorResponseType(typeof(ApiResponse<CategoryDto>))]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [HttpGet("1Category")]
        public async Task<IActionResult> GetCategoryById(Guid categoryId)
        {
            if (categoryId == Guid.Empty)
            {
                return BadRequest(ApiResponse<CategoryDto>.Failure("A valid Category ID is required."));
            }

            var category = await _categoryService.GetCategoryAsync(categoryId);

            if (category == null)
            {
                return NotFound(ApiResponse<CategoryDto>.Failure("Category not found."));
            }

            return Ok(ApiResponse<CategoryDto>.Success(category));
        }

        // --- GetAllCategories ---
        [ProducesErrorResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>))]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
        [HttpGet("AllCategory")]
        [AllowAnonymous]

        public async Task<IActionResult> GetAllAsync()
        {
            var categories = await _categoryService.GetAllAsync();

            if (categories == null || !categories.Any())
            {
                return Ok(ApiResponse<IEnumerable<CategoryDto>>.Success(new List<CategoryDto>(), "No categories found."));
            }

            return Ok(ApiResponse<IEnumerable<CategoryDto>>.Success(categories));
        }
    }
}
