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

        [Display(Name = "Ảnh Poster")]
        public string? PosterUrl { get; set; } // Thay thế ImageUrl

        [Display(Name = "File Âm thanh")]
        public string? AudioUrl { get; set; } // Thêm trường cho file âm thanh quảng cáo

        [StringLength(255)]
        [Display(Name = "Liên kết nhà quảng cáo")]
        public string? AdvertiserLink { get; set; } // Thay thế Link

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Hiển thị")]
        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        [Display(Name = "Thời lượng")]
        public string? Duration { get; set; } // Thêm thời lượng quảng cáo (optional)
    }
}