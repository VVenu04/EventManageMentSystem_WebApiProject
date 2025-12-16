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
        private readonly IServiceItemRepository _serviceItemRepo;


        public CategoryService(ICategoryRepository categoryRepo, IServiceItemRepository serviceItemRepo)
        {
            _categoryRepo = categoryRepo;
            _serviceItemRepo = serviceItemRepo; 
        }

        // --- ADD CATEGORY ---
        public async Task<CategoryDto> AddCategoryAsync(CategoryDto categoryDTO)
        {
            if (categoryDTO == null)
            {
                throw new ArgumentNullException(nameof(categoryDTO));
            }

            // DTO -> Entity Mapping
            var category = new Category
            {
                CategoryID = Guid.NewGuid(),
                CategoryName = categoryDTO.Name
            };

            var addedCategory = await _categoryRepo.AddAsync(category);

            // Entity -> DTO Mapping
            return new CategoryDto
            {
                CategoryID = addedCategory.CategoryID,
                Name = addedCategory.CategoryName
            };
        }


        // --- DELETE CATEGORY ---
        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null)
            {
                throw new Exception("Category not found");
            }

            var servicesUsingCategory = await _serviceItemRepo.GetByCategoryIdAsync(id);

            foreach (var service in servicesUsingCategory)
            {
                service.CategoryID = null; 

                await _serviceItemRepo.UpdateAsync(service);
            }

            await _categoryRepo.DeleteAsync(category);
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
                    Name = cat.CategoryName
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
                Name = category.CategoryName
            };
        }

    }
}
