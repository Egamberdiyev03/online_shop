using Application.DTOs.Category;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {

            _categoryService = categoryService;
        }

        [HttpPost("Create")]
        public async Task<CategoryDto>  CreateCategory (CreateCategoryDto dto)
        {
           return await _categoryService.CreateCategory(dto);
        }

        [HttpGet("GetById")]
        public async Task<ResponseModel<CategoryDto>> GetById(int id)
        {
           return await _categoryService.GetById(id);
        }

        [HttpGet("GetAll")]
        public async Task<List<CategoryDto>> GetAll()
        {
          return await  _categoryService.GetAll();
        }

        [HttpPut("Update")]
        public async Task<ResponseModel<CategoryDto>> UpdateCategory (UpdateCategoryDto dto)
        {
           return await _categoryService.UpdateCategory(dto);
        }

        [HttpDelete("Delete")]
        public async Task<bool> DeleteCategory (int id)
        {
          return await  _categoryService.DeleteCategory(id);
        }
    }
}
