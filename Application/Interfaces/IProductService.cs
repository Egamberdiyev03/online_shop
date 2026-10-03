using Application.DTOs.Product;
using Application.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto> CreateProduct(CreateProductDto dto);
        Task<ResponseModel<List<ProductDto>>> GetAll();
        Task<ResponseModel<PagedResult<ProductDto>>> GetPagedProducts(int pageNumber = 1, int pageSize = 12, string? search = null, int? categoryId = null, int? branchId = null);
        Task<ResponseModel<ProductDto>> GetById(int id);
        Task<ResponseModel<ProductDto>> UpdateProduct(UpdateProductDto dto);
        Task<bool> Deleteproduct(int id);
    }
}
