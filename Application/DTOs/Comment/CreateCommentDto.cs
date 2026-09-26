using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Comment
{
    public class CreateCommentDto
    {
        public string Content { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }

        [Range(1, 5, ErrorMessage = "Baho 1 dan 5 gacha bo'lishi kerak")]
        public int StarRating { get; set; } = 0;
    }
}
