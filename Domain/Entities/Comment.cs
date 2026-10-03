using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; } 
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set;}
      //  public  bool IsActive { get; set; }
        public int StarRating { get; set; } = 0;

        //admin id

        public User User { get; set; }
        public Product Product { get; set; }
    }
}
