using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shopify.Models
{
    public class Playlist
    {
        [Key]
        public int PlaylistId { get; set; }

        [Required]
        public int UserId { get; set; }   // Dùng int theo model User của bạn

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public User User { get; set; }
        public ICollection<PlaylistSong> PlaylistSongs { get; set; }
    }
}
