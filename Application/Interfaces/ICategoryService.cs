using Application.DTOs.Category;
using Application.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
        public   interface ICategoryService
       {
        Task<CategoryDto> CreateCategory(CreateCategoryDto category);
        Task<List<CategoryDto>> GetAll();
        Task<ResponseModel<CategoryDto>> GetById(int id);
        Task<ResponseModel<CategoryDto>> UpdateCategory(UpdateCategoryDto category);
        Task<bool> DeleteCategory(int categoryId);
    }
}
