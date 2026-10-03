using Application.DTOs.Category;
using Application.Extentions;
using Application.Interfaces;
using DataAccess.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _repository;


        public CategoryService(IRepository<Category> repository)
        {
            _repository = repository;
        }

        public async Task<CategoryDto> CreateCategory (CreateCategoryDto category)
        {
            var categoryEntity = new Category
            {
                Title = category.Title,
                Description = category.Description,
                IsActive = category.isActive
            };
            await _repository.AddAsync(categoryEntity);
            await _repository.SaveChangesAsync();

            return new CategoryDto
            {
                Id = categoryEntity.Id,
                Title = categoryEntity.Title,
                Description = categoryEntity.Description,
                isActive = category.isActive
            };
        }
        public async Task<List<CategoryDto>> GetAll()
        {
            var categories = await _repository.GetAsQueryable()
                .Where(c => c.IsActive == true)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    isActive=c.IsActive
                }).ToListAsync();

            if(categories==null || categories.Count==0)
                return new List<CategoryDto>();

            return categories;
        }
        public async Task<ResponseModel<CategoryDto>> GetById(int id)
        {
            var category=await _repository.GetByIdAsync(id);

            if (category == null)
                return   new($"Category {id} topilmadi",HttpStatusCode.NotFound);

            return new(new CategoryDto
            {
                Id = category.Id,
                Title = category.Title,
                Description = category.Description
            });
        }


        public async Task<ResponseModel<CategoryDto>> UpdateCategory(UpdateCategoryDto category)
        {
            var updatecategory= await _repository.GetByIdAsync(category.Id);

            if (updatecategory == null)
                return new($"category {category.Title} topilmadi",HttpStatusCode.NotFound);

            updatecategory.Title = category.Title;
            updatecategory.Description = category.Description;
            updatecategory.IsActive = category.isActive;

            await _repository.SaveChangesAsync();

            return new(new CategoryDto
            {
                Id = updatecategory.Id,
                Title = updatecategory.Title,
                Description = updatecategory.Description,
                isActive = updatecategory.IsActive
            });
        }

        public Task<bool> DeleteCategory (int categoryId)
        {
            return _repository.DeleteAsync(categoryId);
        }
    }
}
