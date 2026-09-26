using Application.DTOs.Comment;
using Application.Extentions;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost("Create")]
        public async Task<ResponseModel<CommentDto>> CreateComment (CreateCommentDto dto)
        {
          return await  _commentService.CreateComment(dto);
        }
        [HttpGet("GetById")]
        public async Task<ResponseModel<CommentDto>> GetCommentById (int id)
        {
          return  await _commentService.GetCommentById(id);
        }
        [HttpGet("GetAll")]
        public async Task<List<CommentDto>> GetAllComment ()
        {
          return await  _commentService.GetAllComment();
        }

        [HttpPut("Update")]
        public async Task<ResponseModel<CommentDto>> UpdateComment(UpdateCommentDto dto)
        {
             return await  _commentService.UpdateComment(dto);
        }

        [HttpDelete("Delete")]
        public async Task<bool> DeleteComment(int id)
        {
           return await _commentService.DeleteComment(id);
        }

        [HttpGet("GetByProductId")]
        public async Task<ResponseModel<List<CommentDto>>> GetCommentsByProductId(int productId)
        {
            return await _commentService.GetCommentsByProductId(productId);
        }

        [HttpGet("GetByCustomerId")]
        public async Task<ResponseModel<List<CommentDto>>> GetCommentsByCustomerId(int customerId)
        {
            return await _commentService.GetCommentsByCustomerId(customerId);
        }

        [HttpGet("GetAverageRating")]
        public async Task<ResponseModel<double>> GetAverageRatingByProductId(int productId)
        {
            return await _commentService.GetAverageRatingByProductId(productId);
        }
    }
}
