using System;
using System.ComponentModel.DataAnnotations;
namespace Shopify.Models
{

    public class Song
    {
        [Key]
        public int SongId { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; }

        public string? Description { get; set; }

        public string? AudioUrl { get; set; }

        public String? Duration { get; set; }
        public string? ImageUrl { get; set; }

        public bool IsVip { get; set; } = false;
        public int PlayCount { get; set; } = 0;

        // FK - Artist
        public int ArtistId { get; set; }
        public Artist? Artist { get; set; }

        // FK - Genre
        public int GenreId { get; set; }
        public Genre? Genre { get; set; }

        // FK - Album
        public int? AlbumId { get; set; }
        public Album? Album { get; set; }
    }
}
