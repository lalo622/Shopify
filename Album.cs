using System;
using System.ComponentModel.DataAnnotations;

namespace Shopify.Models
{
    public class Album
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        [StringLength(150)]
        public string Artist { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [Range(0, 9999)]
        public decimal Price { get; set; }

        [StringLength(100)]
        public string Genre { get; set; }
    }
}
