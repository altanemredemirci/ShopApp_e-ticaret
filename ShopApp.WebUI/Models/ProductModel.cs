using ShopApp.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShopApp.WebUI.Models
{
    public class ProductModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(60, MinimumLength = 10, ErrorMessage ="Ürün ismi min 10 karakter olmalıdır.")]
        public string Name { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        [Required]
        [StringLength(10000, MinimumLength = 20, ErrorMessage = "Ürün açıklaması min 20 karakter olmalıdır.")]
        public string Description { get; set; }

        [Required (ErrorMessage = "Fiyat belirtiniz")]
        [Range(1, 20000)]
        public decimal Price { get; set; }

        public List<Category> SelectedCategories { get; set; }
    }
}
