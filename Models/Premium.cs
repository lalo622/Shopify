using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopify.Models
{
    [Table("Premiums")]
    public class Premium
    {
        [Key]
        public int PremiumId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên gói VIP")]
        [StringLength(100)]
        [Display(Name = "Tên gói")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá gói")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá không hợp lệ")]
        [Display(Name = "Giá (VNĐ)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời hạn (ngày)")]
        [Range(1, 3650, ErrorMessage = "Thời hạn phải từ 1 đến 3650 ngày")]
        [Display(Name = "Thời hạn (ngày)")]
        public int DurationDays { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;
    }
}
