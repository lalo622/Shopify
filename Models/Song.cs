using System;
using System.ComponentModel.DataAnnotations;

public class Song
{
    [Key]
    public int SongId { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public string AudioUrl { get; set; }

    public TimeSpan? Duration { get; set; }

    // FK - Artist
    public int ArtistId { get; set; }
    public Artist? Artist { get; set; }

    // FK - Genre
    public int GenreId { get; set; }
    public Genre? Genre { get; set; }

    // FK - Album
    public int AlbumId { get; set; }
    public Album? Album { get; set; }
}
