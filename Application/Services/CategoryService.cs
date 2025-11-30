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
            // (EventMapper-ல் செய்தது போல் CategoryMapper-லும் MapToCategory இருக்க வேண்டும்)
            var category = new Category
            {
                CategoryID = Guid.NewGuid(),
                CategoryName = categoryDTO.Name
                // வேறு Properties இருந்தால் இங்கே சேர்க்கவும்
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

            // 2. 🚨 இந்த Category-ஐப் பயன்படுத்தும் எல்லா Service-களையும் கண்டுபிடி
            var servicesUsingCategory = await _serviceItemRepo.GetByCategoryIdAsync(id);

            // 3. 🚨 அந்த Service-களில் இருந்து Category-ஐ நீக்கு (Unlink)
            // (Service.cs-ல் CategoryID Nullable 'Guid?' ஆக இருக்க வேண்டும்)
            foreach (var service in servicesUsingCategory)
            {
                service.CategoryID = null; // அல்லது null (Guid? ஆக இருந்தால்)

                // குறிப்பு: உங்கள் Service Entity-ல் CategoryID 'Guid' (Not Null) ஆக இருந்தால், 
                // நீங்கள் ஒரு 'Default/General' Category ID-ஐப் பயன்படுத்தலாம்.
                // அல்லது Service Entity-ல் 'Guid?' (Nullable) என மாற்றினால் 'null' போடலாம்.

                await _serviceItemRepo.UpdateAsync(service);
            }

            // 4. இப்போது Category-ஐத் தைரியமாக அழிக்கலாம்
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
