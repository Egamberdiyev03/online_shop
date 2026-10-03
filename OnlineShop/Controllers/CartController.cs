using Application.DTOs.Cart;
using Application.Extentions;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("Create")]
        public async Task<ResponseModel<CartDto>> CreateCart (CreateCartDto dto)
        {
          return await  _cartService.CreateCartAsync(dto);
        }

        [HttpGet("GetById")]
        public async Task<ResponseModel<CartDto>> GetById (int id)
        {
           return await _cartService.GetCartByIdAsync(id);
        }
        [HttpGet("GetAll")] 
        public async Task<ResponseModel<List<CartDto>>> Getall ()
        {
            return await _cartService.GetAllCartsAsync();
        }

        [HttpDelete("Delete")]
        public async Task<bool> DeleteCart(int id)
        {
           return await _cartService.DeleteCartAsync(id);
        }

        [HttpPost("AddItemtoCart")]
        public async Task<ResponseModel<bool>> AddCartItemAsync(int UserId,int productId, int quantity)
        {
          return await  _cartService.AddItemToCartAsync(UserId, productId, quantity);
        }

        [HttpGet("GetByCustomerIdCart")]
        public Task<ResponseModel<CartDto>> GetByUserIdCart(int UserId)
        {
           return  _cartService.GetCartByUserIdAsync(UserId);
        }

        [HttpPut("Update")]
        public async Task<ResponseModel<bool>> UpdateCart(int UserId, int productId, int quantity)
        {
           return await _cartService.UpdateItemQuantityAsync( UserId, productId, quantity);
        }

        [HttpDelete("RemoveItemfromCart")]
        public async Task<ResponseModel<bool>> RemoveItemfromCartToProduct(int UserId,int productId)
        {
           return await _cartService.RemoveItemFromCartAsync(UserId, productId);
        }

    }

}
