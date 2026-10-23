using Application.DTOs.Product;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IProductService _service;
        public ProductController(IProductService service)
        {
            _service = service;
        }

        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPost("Create")]
        public async Task<ProductDto> CreateProduct(CreateProductDto dto)
             => await _service.CreateProduct(dto);

        [HttpGet("GetById")]
        public async Task<ResponseModel<ProductDto>> GetProductById(int id)
             => await _service.GetById(id);

        [HttpGet("GetAll")]
        public async Task<ResponseModel<List<ProductDto>>> GetAllProduct()
             => await _service.GetAll();

        [HttpGet("GetPaged")]
        public async Task<ResponseModel<PagedResult<ProductDto>>> GetPagedProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] int? branchId = null)
             => await _service.GetPagedProducts(pageNumber, pageSize, search, categoryId, branchId);

        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPut("Update")]
        public async Task<ResponseModel<ProductDto>> UpdateProduct(UpdateProductDto dto)
             => await _service.UpdateProduct(dto);

        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpDelete("Delete")]
        public async Task<bool> DeleteProduct(int id)
             => await _service.Deleteproduct(id);

    }

}