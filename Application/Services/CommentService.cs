using Application.DTOs.Comment;
using Application.Extentions;
using Application.Interfaces;
using AutoMapper;
using DataAccess.Repositories;
using Domain.Entities;
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
        private readonly IMapper _mapper;

        public CommentService(IRepository<Comment> commentRepository,IRepository<Product> productRepository,IMapper mapper)
        {
            _commentRepository = commentRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ResponseModel<CommentDto>> CreateComment (CreateCommentDto dto)
        {
            var product = await  _productRepository.GetByIdAsync(dto.ProductId);

            if (product == null)
                return new($"Product mavjud emas", HttpStatusCode.BadRequest);

            var comment = _mapper.Map<Comment>(dto);
            comment.CreatedAt = DateTime.UtcNow;

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
                    CustomerId = c.CustomerId,
                    ProductId = c.ProductId,
                    StarRating = c.StarRating,
                    CustomerName = c.Customer.Name
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


        public async Task<bool> DeleteComment (int id)
        {
            return await _commentRepository.DeleteAsync(id); 
        }

        public async Task<ResponseModel<CommentDto>> UpdateComment (UpdateCommentDto dto)
        {
            var comment =  await _commentRepository.GetByIdAsync(dto.Id);

            if (comment == null)
                return  new($"Comment {dto.Id} topilmadi",HttpStatusCode.NotFound);

            comment.Content = dto.Content;
            comment.StarRating = dto.StarRating;
            comment.UpdatedAt = DateTime.UtcNow;
            await _commentRepository.SaveChangesAsync();

            return new(_mapper.Map<CommentDto>(comment));
        }

        public async Task<ResponseModel<List<CommentDto>>> GetCommentsByProductId(int productId)
        {
            var comments = await _commentRepository.GetAsQueryable()
                .Where(c => c.ProductId == productId)
                .Select(c => new CommentDto
                {
                    ProductId = c.ProductId,
                    CustomerId = c.CustomerId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    StarRating = c.StarRating
                }).ToListAsync();

            if (!comments.Any())
                return new($"Bu productga tegishli commmentlar topilmadi", HttpStatusCode.NotFound);

            return new(comments); 
        }

        public async Task<ResponseModel<List<CommentDto>>> GetCommentsByCustomerId(int customerId)
        {
            var comments = await _commentRepository.GetAsQueryable()
               .Where(c => c.CustomerId==customerId)
               .Select(c => new CommentDto
               {
                   ProductId = c.ProductId,
                   CustomerId = c.CustomerId,
                   Content = c.Content,
                   CreatedAt = c.CreatedAt,
                   UpdatedAt = c.UpdatedAt,
                   StarRating = c.StarRating
               }).ToListAsync();

            if (!comments.Any())
                return new($"Bu productga tegishli commmentlar topilmadi", HttpStatusCode.NotFound);

            return new(comments);
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
