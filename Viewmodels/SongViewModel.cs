using System;

namespace Shopify.Models.ViewModels
{
    public class SongDetailViewModel
    {
        public int SongId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AudioUrl { get; set; }
        public string? Duration { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsVip { get; set; }
        public int PlayCount { get; set; }

        public string ArtistName { get; set; } = string.Empty;
        public string GenreName { get; set; } = string.Empty;
        public string? AlbumTitle { get; set; }
    }
}
