using Application.DTOs.Cart;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design.Serialization;

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
        public async Task<CartDto> CreateCart (CreateCartDto dto)
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
        public async Task<ResponseModel<bool>> AddCartItemAsync(int customerId,int productId, int quantity)
        {
          return await  _cartService.AddItemToCartAsync(customerId, productId, quantity);
        }

        [HttpGet("GetByCustomerIdCart")]
        public Task<ResponseModel<CartDto>> GetByCustomerIdCart(int customerId)
        {
           return  _cartService.GetCartByCustomerIdAsync(customerId);
        }

        [HttpPut("Update")]
        public async Task<ResponseModel<bool>> UpdateCart(int customerId, int productId, int quantity)
        {
           return await _cartService.UpdateItemQuantityAsync( customerId, productId, quantity);
        }

        [HttpDelete("RemoveItemfromCart")]
        public async Task<ResponseModel<bool>> RemoveItemfromCartToProduct(int customerId,int productId)
        {
           return await _cartService.RemoveItemFromCartAsync(customerId, productId);
        }

    }

}
