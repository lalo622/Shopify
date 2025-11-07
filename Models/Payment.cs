using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopify.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Display(Name = "Ngày thanh toán")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Số tiền (VNĐ)")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền không hợp lệ")]
        public decimal? Amount { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        [StringLength(50)]
        public string Method { get; set; } = "VNPay";

        [Display(Name = "Trạng thái")]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed

        // ----------------- Khóa ngoại -----------------
        [Display(Name = "Người dùng")]
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Display(Name = "Gói Premium")]
        public int? PremiumId { get; set; }

        [ForeignKey("PremiumId")]
        public Premium? Premium { get; set; }

        // ----------------- Thông tin VNPay -----------------
        [Display(Name = "Mã giao dịch VNPay")]
        [StringLength(100)]
        public string? TransactionId { get; set; }

        [Display(Name = "Mã phản hồi VNPay")]
        [StringLength(10)]
        public string? ResponseCode { get; set; }
    }
}
