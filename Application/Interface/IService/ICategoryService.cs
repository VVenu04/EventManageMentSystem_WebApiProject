using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface ICategoryService
    {
        Task<CategoryDto> AddCategoryAsync(CategoryDto categoryDTO);
        Task DeleteCategoryAsync(Guid Id);
        Task<CategoryDto> GetCategoryAsync(Guid categoryId);
        Task<IEnumerable<CategoryDto>> GetAllAsync();
    }
}
