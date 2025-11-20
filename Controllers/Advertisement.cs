using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopify.Models
{
    [Table("Advertisements")]
    public class Advertisement
    {
        [Key]
        public int AdvertisementId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề quảng cáo")]
        [StringLength(100)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Ảnh quảng cáo")]
        public string? ImageUrl { get; set; }

        [StringLength(255)]
        [Display(Name = "Liên kết")]
        public string? Link { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Hiển thị")]
        public bool IsActive { get; set; } = true;
    }
}
