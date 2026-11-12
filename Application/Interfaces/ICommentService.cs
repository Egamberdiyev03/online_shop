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
        Task<ResponseModel<PagedResult<CommentDto>>> GetAllComment(PagedQuery filter);
        Task<ResponseModel<CommentDto>> GetCommentById(int id);
        Task<ResponseModel<CommentDto>> CreateComment(CreateCommentDto dto,int userId);
        Task<bool> DeleteComment(int id,int userId,bool IsAdmin);
        Task<ResponseModel<CommentDto>> UpdateComment(UpdateCommentDto dto);

        Task<ResponseModel<PagedResult<CommentDto>>> GetCommentsByProductId(int productId, PagedQuery filter);
        Task<ResponseModel<PagedResult<CommentDto>>> GetCommentsByUserId(int UserId,PagedQuery filter);
        Task<ResponseModel<double>> GetAverageRatingByProductId(int productId);
    }
}
