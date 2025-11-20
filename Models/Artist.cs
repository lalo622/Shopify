using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Shopify.Models {
    public class Artist {
        [Key] 
        public int Id { get; set; }
        [Required, StringLength(100)]
        [Display(Name = "Tên ca sĩ")] 
        public string Name { get; set; } 
        [StringLength(200)][Display(Name = "Quốc gia")] 
        public string QuocGia { get; set; } [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)] public DateTime? NgaySinh { get; set; } 
        [Display(Name = "Tiểu sử")] public string MoTa { get; set; } 
        [Display(Name = "Ảnh ca sĩ")] public string? ImageUrl { get; set; } 
        [Display(Name = "Hiển thị")] public bool IsActive { get; set; } = true; // true = hiển thị, false = ẩn
        public ICollection<Song>? Songs { get; set; }
        public ICollection<Album>? Albums { get; set; } } }