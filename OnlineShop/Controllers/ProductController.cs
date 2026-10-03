using Application.DTOs.Product;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        
        public ProductController(IProductService service)
        {
            _service = service;
        }

        [HttpPost("Create")]
        public async Task<ProductDto> CreateProduct (CreateProductDto dto)
        {
           return await _service.CreateProduct(dto);
        }

        [HttpGet("GetById")]
        public async Task<ResponseModel<ProductDto>> GetProductById (int id)
        {
          return await _service.GetById(id);
        }
        
        [HttpGet("GetAll")]
        public async Task<ResponseModel<List<ProductDto>>> GetAllProduct()
        {
           return await _service.GetAll();
        }

        [HttpGet("GetPaged")]
        public async Task<ResponseModel<PagedResult<ProductDto>>> GetPagedProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] int? branchId = null)
        {
            return await _service.GetPagedProducts(pageNumber, pageSize, search, categoryId, branchId);
        }
        
        [HttpPut("Update")]
        public async Task<ResponseModel<ProductDto>> UpdateProduct(UpdateProductDto dto)
        {
          return await _service.UpdateProduct(dto);
        }

        [HttpDelete("Delete")]
        public async Task<bool> DeleteProduct (int id)
        {
           return await _service.Deleteproduct(id);
        }
    }
}
