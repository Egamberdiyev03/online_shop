using Application.DTOs.Cart;
using Application.Extentions;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("Create")]
        public async Task<ResponseModel<CartDto>> CreateCart (CreateCartDto dto)
            => await  _cartService.CreateCartAsync(dto);

        [HttpGet("GetById")]
        public async Task<ResponseModel<CartDto>> GetById (int id)
            => await _cartService.GetCartByIdAsync(id);
        
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpGet("GetAll")] 
        public async Task<ResponseModel<List<CartDto>>> Getall ()
            => await _cartService.GetAllCartsAsync();
            
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpDelete("Delete")]
        public async Task<bool> DeleteCart(int id)
            => await _cartService.DeleteCartAsync(id);
        
        [HttpPost("AddItemtoCart")]
        public async Task<ResponseModel<bool>> AddCartItemAsync(int UserId,int productId, int quantity)
            => await  _cartService.AddItemToCartAsync(UserId, productId, quantity);
        
        [HttpGet("GetByCustomerIdCart")]
        public async Task<ResponseModel<CartDto>> GetByUserIdCart(int UserId)
            => await _cartService.GetCartByUserIdAsync(UserId);

        [HttpPut("Update")]
        public async Task<ResponseModel<bool>> UpdateCart(int UserId, int productId, int quantity)
            => await _cartService.UpdateItemQuantityAsync(UserId, productId, quantity);

        [HttpDelete("RemoveItemfromCart")]
        public async Task<ResponseModel<bool>> RemoveItemfromCartToProduct(int UserId,int productId)
            => await _cartService.RemoveItemFromCartAsync(UserId, productId);
    }
}
