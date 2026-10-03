using Application.DTOs.Comment;
using Application.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICommentService
    {
        //CRUD
        Task<List<CommentDto>> GetAllComment();
        Task<ResponseModel<CommentDto>> GetCommentById(int id);
        Task<ResponseModel<CommentDto>> CreateComment(CreateCommentDto dto);
        Task<bool> DeleteComment(int id);
        Task<ResponseModel<CommentDto>> UpdateComment(UpdateCommentDto dto);


        Task<ResponseModel<List<CommentDto>>> GetCommentsByProductId(int productId);
        Task<ResponseModel<List<CommentDto>>> GetCommentsByUserId(int UserId);
        Task<ResponseModel<double>> GetAverageRatingByProductId(int productId);
    }
}
