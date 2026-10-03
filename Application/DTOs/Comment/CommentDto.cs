using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Comment
{
    public  class CommentDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        // public bool IsActive { get; set; }
        public int StarRating { get; set; } = 0;


    }
}
