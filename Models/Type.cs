using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Genre
{
    [Key]
    public int GenreId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    public string? Description { get; set; }

    // Navigation
    public ICollection<Song>? Songs { get; set; }
}
