using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.DTOs.Cart;    

namespace Application.Interfaces
{
    public interface ICartService   
    {
        Task<IEnumerable<CartDto>> GetAllCartsAsync();
        Task<CartDto> GetCartByIdAsync(int id);
        Task<CreateCartDto> CreateCartAsync(CartDto cartDto);
        Task DeleteCartAsync(int id);
        Task<CartDto> GetCartByCustomerIdAsync(int customerId);
        Task<bool> AddItemToCartAsync(int customerId, int productId, int quantity);
        Task<bool> UpdateItemQuantityAsync(int customerId, int productId, int quantity);
        Task<bool> RemoveItemFromCartAsync(int customerId, int productId);
        Task<bool> ClearCartAsync(int customerId);
    }
}
