using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyWeb2.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        [DisplayName("Category Name")]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 100,ErrorMessage ="1 ile 100 arsında olmalı")]
        public int DisplayOrder { get; set; }
        ICollection<Product> Products;


    }
}
