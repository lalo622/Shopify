using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Artist
{
    [Key]
    public int ArtistId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; }

    public string? Bio { get; set; }
    public string? ImageUrl { get; set; }

    // Navigation
    public ICollection<Song>? Songs { get; set; }
    public ICollection<Album>? Albums { get; set; }
}
