using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Cart;
using Application.Extentions;
using DataAccess.Repositories;

namespace Application.Interfaces
{
    public interface ICartService 
    {
        Task<ResponseModel<CartDto>> CreateCartAsync(CreateCartDto cartDto);
        Task<ResponseModel<List<CartDto>>> GetAllCartsAsync();
        Task<ResponseModel<CartDto>> GetCartByIdAsync(int id);
        Task<bool> DeleteCartAsync(int id);


        Task<ResponseModel<CartDto>> GetCartByUserIdAsync(int userId);
        Task<ResponseModel<bool>> AddItemToCartAsync(int userId, int productId, int quantity);
        Task<ResponseModel<bool>> UpdateItemQuantityAsync(int userId, int productId, int quantity);
        Task<ResponseModel<bool>> RemoveItemFromCartAsync(int userId, int productId);
        Task<bool> ClearCartAsync(int userId);
    }
}
