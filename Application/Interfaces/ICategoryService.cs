using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.DTOs.Category;

namespace Application.Interfaces
{
        public   interface ICategoryService
       {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CreateCategoryDto> CreateCategoryAsync(CreateCategoryDto categoryDto);
        Task<CategoryDto> GetCategoryByIdAsync(int categoryId); 
        Task<CategoryDto> GetCategoryByNameAsync(string name);
        Task<UpdateCategoryDto> UpdateCategoryAsync(int categoryId, UpdateCategoryDto categoryDto);
    }
}
