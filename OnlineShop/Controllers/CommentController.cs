using Application.DTOs.Comment;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return await _commentService.CreateComment(dto, userId);    
        }

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

        [Authorize]
        [HttpDelete("Delete")]
        public async Task<bool> DeleteComment(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("CompanyAdmin");
            return  await _commentService.DeleteComment(id,userId, isAdmin);
        }
        
        [HttpGet("GetByProductId")]
        public async Task<ResponseModel<PagedResult<CommentDto>>> GetCommentsByProductId(int productId, int pageNumber, int pageSize)
            => await _commentService.GetCommentsByProductId(productId, pageNumber, pageSize);

        [HttpGet("GetByCustomerId")]
        public async Task<ResponseModel<List<CommentDto>>> GetCommentsByUserId(int customerId)
            => await _commentService.GetCommentsByUserId(customerId);
        
        [HttpGet("GetAverageRating")]
        public async Task<ResponseModel<double>> GetAverageRatingByProductId(int productId)
            => await _commentService.GetAverageRatingByProductId(productId);
    }
}
