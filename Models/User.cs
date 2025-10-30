using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopify.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người dùng")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(255)]
        public string PasswordHash { get; set; }  

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "User"; 

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now; 

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true; 
    }
}
