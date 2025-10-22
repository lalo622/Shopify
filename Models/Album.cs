using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Album
{
    [Key]
    public int AlbumId { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; }

    public DateTime ReleaseDate { get; set; }
    public string? CoverImageUrl { get; set; }

    // Navigation
    public int ArtistId { get; set; }
    public Artist? Artist { get; set; }

    public ICollection<Song>? Songs { get; set; }
}
