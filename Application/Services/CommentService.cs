using Application.DTOs.Comment;
using Application.Extentions;
using Application.Interfaces;
using AutoMapper;
using DataAccess.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly IRepository<Comment> _commentRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IMapper _mapper;

        public CommentService(IRepository<Comment> commentRepository,IRepository<Product> productRepository,IRepository<Order> orderRepository,IMapper mapper)
        {
            _commentRepository = commentRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<ResponseModel<CommentDto>> CreateComment (CreateCommentDto dto,int userId)
        {
            if (dto.StarRating < 1 || dto.StarRating > 5)
                return new("Baho 1 dan 5 gacha bo'lishi kerak", HttpStatusCode.BadRequest);

            var product = await  _productRepository.GetByIdAsync(dto.ProductId);

            if (product == null)
                return new($"Product mavjud emas", HttpStatusCode.BadRequest);

            var purchased = await _orderRepository.GetAsQueryable()
                        .AnyAsync(o => o.UserId == userId
                    && o.Status == OrderStatus.Delivered
                    && o.OrderItems.Any(i => i.ProductId == dto.ProductId));

            if (!purchased)
                return new("Sharh qoldirish uchun mahsulotni sotib olgan bo'lishingiz kerak", HttpStatusCode.Forbidden);

            var alreadyReviewed = await _commentRepository.GetAsQueryable()
                   .AnyAsync(c => c.UserId == userId && c.ProductId == dto.ProductId);

            if (alreadyReviewed)
                return new("Siz bu mahsulotga allaqachon sharh yozgansiz", HttpStatusCode.Conflict);

            var comment = _mapper.Map<Comment>(dto);
            comment.UserId = userId;
            comment.CreatedAt = DateTime.UtcNow.AddHours(5);

                await  _commentRepository.AddAsync(comment);
                await  _commentRepository.SaveChangesAsync();

            return new(_mapper.Map<CommentDto>(comment));
        }
        public async Task<List<CommentDto>> GetAllComment()
        {
            var comments = await _commentRepository.GetAsQueryable().
                Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserId = c.UserId,
                    ProductId = c.ProductId,
                    StarRating = c.StarRating,
                    UserName = c.User.Name
                }).ToListAsync();

            if(comments==null || !comments.Any()) return new List<CommentDto>();
            
            return comments;
        }

        public async Task<ResponseModel<CommentDto>> GetCommentById (int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if(comment==null)
                return  new($"comment {id} topilmadi",HttpStatusCode.NotFound);

            return new(_mapper.Map<CommentDto>(comment));  
        }


        public async Task<bool> DeleteComment (int id,int userId,bool isAdmin)
        {
            var exists = await _commentRepository.GetAsQueryable().AnyAsync(f => f.Id == id && (isAdmin || f.UserId == userId));
            if (!exists)
                return false;

            return await _commentRepository.DeleteAsync(id); 
        }

        public async Task<ResponseModel<CommentDto>> UpdateComment (UpdateCommentDto dto)
        {
            var comment =  await _commentRepository.GetByIdAsync(dto.Id);

            if (comment == null)
                return  new($"Comment {dto.Id} topilmadi",HttpStatusCode.NotFound);

            comment.Content = dto.Content;
            comment.StarRating = dto.StarRating;
            comment.UpdatedAt = DateTime.UtcNow.AddHours(5);
            await _commentRepository.SaveChangesAsync();

            return new(_mapper.Map<CommentDto>(comment));
        }

        public async Task<ResponseModel<PagedResult<CommentDto>>> GetCommentsByProductId(int productId, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var query =  _commentRepository.GetAsQueryable()
                .AsNoTracking()
                .Where(c => c.ProductId == productId);


            var totalCount = await query.CountAsync();

            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .ThenByDescending(c=>c.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    UserId = c.UserId,
                    UserName = c.User.Name,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    StarRating = c.StarRating
                }).ToListAsync();

            var paged = new PagedResult<CommentDto>
            {
                Items = comments,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return new("Muvaffaqiyatli", HttpStatusCode.OK, paged);
        }

        public async Task<ResponseModel<PagedResult<CommentDto>>> GetCommentsByUserId(int UserId, int pageNumber, int pageSize )
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;


            var query = _commentRepository.GetAsQueryable()
               .AsNoTracking()
               .Where(c => c.UserId == UserId);
               
            var totalCount = await query.CountAsync();

            var comments = await query
               .OrderByDescending(c => c.CreatedAt)
               .ThenByDescending(c => c.Id)
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .Select(c => new CommentDto
               {
                   Id = c.Id,
                   ProductId = c.ProductId,
                   UserId = c.UserId,
                   UserName = c.User.Name,
                   Content = c.Content,
                   CreatedAt = c.CreatedAt,
                   UpdatedAt = c.UpdatedAt,
                   StarRating = c.StarRating
               }).ToListAsync();

            var paged = new PagedResult<CommentDto>
            {
                Items = comments,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return new(paged);
        }

        public async Task<ResponseModel<double>> GetAverageRatingByProductId(int productId)
        {
            var productRatings = _commentRepository.GetAsQueryable()
                .Where(p => p.ProductId == productId)
                .Select(c => c.StarRating);

            if (!await productRatings.AnyAsync())
                return new("Bu productga tegishli comment topilmadi",HttpStatusCode.NotFound);

            double average = await productRatings.AverageAsync(r => r);
           
            return new(average);
        }
    }
}
