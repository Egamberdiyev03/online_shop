

using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace Domain.Entities
{
    //[Table("products")]
    public class Product
    {
        //[Column("id")]
        public int Id { get; set; }
        //[Column("name")]
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Quantity { get; set; } = 0;
        public int CompanyBranchId { get; set; }


        [Column("product_image")]
        public string Image { get; set; }
        public int? CreatedBy { get; set; }

        public Category Category { get; set; }
        public List<Comment> Comments { get; set; }
        public CompanyBranch CompanyBranch { get; set; }
    }
}
