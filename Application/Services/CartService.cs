using Application.DTOs.Cart;
using Application.Extentions;
using Application.Interfaces;
using DataAccess.Database;
using DataAccess.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Reflection.Metadata.Ecma335;

namespace Application.Services
{
    public class CartService : ICartService
    {
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Product> _prooductRepository;
        private readonly IRepository<CartItem> _cartItemRepository;

        public CartService
            (
            IRepository<Cart> cartRepository,
            IRepository<Product> productRepository,
            IRepository<CartItem> cartItemRepository)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _prooductRepository = productRepository;
        }
        public async Task<ResponseModel<CartDto>> CreateCartAsync(CreateCartDto cart)
        {
            var existingCart = await _cartRepository.GetAsQueryable()
                .FirstOrDefaultAsync(d => d.UserId == cart.UserId);

            if (existingCart == null)
                return new("User Topilmadi", HttpStatusCode.BadRequest);

            if(existingCart !=null)
            {
                return new(new CartDto
                {
                    Id = existingCart.Id,
                    UserId = existingCart.UserId,
                    CreatedAt = existingCart.CreatedAt
                });
            }

            var cartEntity = new Cart
            {
                UserId = cart.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _cartRepository.AddAsync(cartEntity);
            await _cartRepository.SaveChangesAsync();

            return new(new CartDto
            {
                Id=cartEntity.Id,
                UserId = cartEntity.UserId,
                CreatedAt = cartEntity.CreatedAt
            });
        }

        public async Task<ResponseModel<List<CartDto>>> GetAllCartsAsync()
        {
            var carts = await _cartRepository.GetAsQueryable()
            .Select(c => new CartDto
            {
                Id = c.Id,
                UserId = c.UserId,
                CreatedAt = c.CreatedAt
            }).ToListAsync();

            return new(carts);
        }

        public async Task<ResponseModel<CartDto>> GetCartByIdAsync(int id)
        {
            var cart = await _cartRepository.GetByIdAsync(id);
            if (cart == null)
                return new($"Cart topilmadi",HttpStatusCode.NotFound);

            return new(new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CreatedAt = cart.CreatedAt
            });
        }

        public async Task<bool> DeleteCartAsync(int id)
        {
            var result = await _cartRepository.DeleteAsync(id);
            if (result)
                await _cartRepository.SaveChangesAsync();
            
            return result;
        }

        public async Task<ResponseModel<bool>> AddItemToCartAsync(int userId, int productId, int quantity)
        {
            var cart = await _cartRepository.GetAsQueryable().FirstOrDefaultAsync(a => a.UserId == userId);
            if(cart == null)
            {
               Cart newCart= new Cart
               {
                   UserId = userId,
                   CreatedAt = DateTime.UtcNow
               };
                await  _cartRepository.AddAsync(newCart);
                await _cartRepository.SaveChangesAsync();
                cart = newCart;
            };

            var product = await _prooductRepository.GetByIdAsync(productId);

            if (product == null)
                 return new($"Product {productId} topilmadi",HttpStatusCode.NotFound);

            var currentCartItem = await _cartItemRepository.GetAsQueryable()
                .FirstOrDefaultAsync(s => s.CartId == cart.Id && s.ProductId == productId);

            var totalRequested = quantity + (currentCartItem?.Quantity ?? 0);

            if (product.Quantity < totalRequested )
                 return new($"Omborda yetarli mahsulot yo'q. Mavjud: {product.Quantity} dona, " +
                     $"so'ralgan: {quantity} dona.",HttpStatusCode.BadRequest);

            if(currentCartItem == null)
            {
                var cartitem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                };

                await _cartItemRepository.AddAsync(cartitem);
            }
            else
            {
                currentCartItem.Quantity += quantity;
            }

            await _cartItemRepository.SaveChangesAsync();
                return new(true);

         //  return new(false);
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var cart = await _cartRepository.GetAsQueryable()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(s=>s.UserId==userId);

            if(cart == null) return false;

            foreach (var item in cart.CartItems)
            {
                await _cartItemRepository.DeleteAsync(item.Id);
            }

            await _cartItemRepository.SaveChangesAsync();

            return true; 
        }

        public async Task<ResponseModel<CartDto>> GetCartByUserIdAsync(int userId)
        {
            var cart = await _cartRepository.GetAsQueryable()
                .Include(s=>s.CartItems)
                .ThenInclude(d=>d.Product)
                .FirstOrDefaultAsync(s => s.UserId == userId);
            if (cart == null)
                 return new($"Customer {userId} uchun cart topilmadi",HttpStatusCode.NotFound);

            return new(new CartDto  
            {
                Id = cart.Id,
                UserId = userId,
                CreatedAt = cart.CreatedAt,
                CartItems= cart.CartItems.Select( c=> new CartItemDto
                {
                    Id=c.Id,
                    CartId=c.CartId,
                    ProductId=c.ProductId,
                    Quantity=c.Quantity,
                    UnitPrice=c.Product.Price
                }).ToList(),
                TotalPrice=cart.CartItems.Sum(i=>i.Product.Price*i.Quantity)
            });
        }

        public async Task<ResponseModel<bool>> RemoveItemFromCartAsync(int userId, int productId)
        {
           
           var cart =await _cartRepository.GetAsQueryable()
                .Include(s=>s.CartItems)
                .FirstOrDefaultAsync(c=>c.UserId == userId);

            if (cart == null)
                return new("Cart topilmadi",HttpStatusCode.NotFound);

            var item = cart.CartItems.FirstOrDefault(c=>c.ProductId==productId);

            if (item == null) 
                return new($"Bu User savatida {productId} idli mahsulot yuq",HttpStatusCode.NotFound);

            cart.CartItems.Remove(item);
           await _cartRepository.SaveChangesAsync();
                return new(true);
        }

        public async Task<ResponseModel<bool>> UpdateItemQuantityAsync(int userId, int productId, int quantity)
        {
            var cart = await _cartRepository
                .GetAsQueryable()
                .Include(s=>s.CartItems)
                .FirstOrDefaultAsync(c=>c.UserId== userId);

            if (cart == null || !cart.CartItems.Any()) 
                return new($"User {userId} uchun cart topilmadi",HttpStatusCode.BadRequest);

            var item = cart.CartItems.FirstOrDefault( c=>c.ProductId==productId);

            if (item == null)
                return new($"Savatda bu mahsulot mavjud emas", HttpStatusCode.BadRequest);

            var product = await _prooductRepository.GetByIdAsync(productId);

            if (product.Quantity < quantity)
               return  new($"Omborda yetarli mahsulot yo'q. Mavjud: {product.Quantity} dona, so'ralgan: {quantity} dona.", HttpStatusCode.BadRequest);

            item.Quantity = quantity;
          
            await _cartRepository.SaveChangesAsync();

            return new(true);   
        }

    }
}
