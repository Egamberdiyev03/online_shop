using Application.DTOs.Category;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPost("Create")]
        public async Task<CategoryDto>  CreateCategory (CreateCategoryDto dto)
            => await _categoryService.CreateCategory(dto);
        
        [HttpGet("GetById")]
        public async Task<ResponseModel<CategoryDto>> GetById(int id)
            => await _categoryService.GetById(id);
        
        [HttpGet("GetAll")]
        public async Task<List<CategoryDto>> GetAll()
            => await  _categoryService.GetAll();
        
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPut("Update")]
        public async Task<ResponseModel<CategoryDto>> UpdateCategory (UpdateCategoryDto dto)
            => await _categoryService.UpdateCategory(dto);
        
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpDelete("Delete")]
        public async Task<bool> DeleteCategory (int id)
            => await  _categoryService.DeleteCategory(id);
    }
}
