using Application.DTOs.Comment;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    public class CommentController : BaseController
    {
        private readonly ICommentService _commentService;
        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost("Create")]
        public async Task<ResponseModel<CommentDto>> CreateComment (CreateCommentDto dto)
            => await _commentService.CreateComment(dto);

        [HttpGet("GetById")]
        public async Task<ResponseModel<CommentDto>> GetCommentById (int id)
            => await _commentService.GetCommentById(id);

        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpGet("GetAll")]
        public async Task<List<CommentDto>> GetAllComment ()
            => await  _commentService.GetAllComment();
        
        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpPut("Update")]
        public async Task<ResponseModel<CommentDto>> UpdateComment(UpdateCommentDto dto)
            => await _commentService.UpdateComment(dto);

        [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
        [HttpDelete("Delete")]
        public async Task<bool> DeleteComment(int id)
            => await _commentService.DeleteComment(id);
        
        [HttpGet("GetByProductId")]
        public async Task<ResponseModel<List<CommentDto>>> GetCommentsByProductId(int productId)
            => await _commentService.GetCommentsByProductId(productId);
        
        [HttpGet("GetByCustomerId")]
        public async Task<ResponseModel<List<CommentDto>>> GetCommentsByUserId(int customerId)
            => await _commentService.GetCommentsByUserId(customerId);
        
        [HttpGet("GetAverageRating")]
        public async Task<ResponseModel<double>> GetAverageRatingByProductId(int productId)
            => await _commentService.GetAverageRatingByProductId(productId);
    }
}
