using Application.DTOs;
using Application.Interface.IRepo;
using Application.Interface.IService;
using Application.Mapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CategoryService: ICategoryService
    {
        private readonly ICategoryRepository _categoryRepo;


        public CategoryService(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        // --- ADD CATEGORY ---
        public async Task<CategoryDto> AddCategoryAsync(CategoryDto categoryDTO)
        {
            if (categoryDTO == null)
            {
                throw new ArgumentNullException(nameof(categoryDTO));
            }

            // DTO -> Entity Mapping
            // (EventMapper-ல் செய்தது போல் CategoryMapper-லும் MapToCategory இருக்க வேண்டும்)
            var category = new Category
            {
                CategoryID = Guid.NewGuid(),
                CategoryName = categoryDTO.CategoryName
                // வேறு Properties இருந்தால் இங்கே சேர்க்கவும்
            };

            var addedCategory = await _categoryRepo.AddAsync(category);

            // Entity -> DTO Mapping
            return new CategoryDto
            {
                CategoryID = addedCategory.CategoryID,
                CategoryName = addedCategory.CategoryName
            };
        }


        // --- DELETE CATEGORY ---
        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category != null)
            {
                await _categoryRepo.DeleteAsync(category);
            }
        }

        // --- GET ALL CATEGORIES ---
        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _categoryRepo.GetAllAsync();

            // Manual Mapping List
            var categoryDtos = new List<CategoryDto>();
            foreach (var cat in categories)
            {
                categoryDtos.Add(new CategoryDto
                {
                    CategoryID = cat.CategoryID,
                    CategoryName = cat.CategoryName
                });
            }
            return categoryDtos;
        }

        // --- GET CATEGORY BY ID ---
        public async Task<CategoryDto> GetCategoryAsync(Guid categoryId)
        {
            var category = await _categoryRepo.GetByIdAsync(categoryId);

            if (category == null)
            {
                return null;
            }

            return new CategoryDto
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName
            };
        }

    }
}
