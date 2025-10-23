using Microsoft.EntityFrameworkCore;
using Shopify.Models;
namespace Shopify.Data
{
    public class MusicDbContext : DbContext
    {
        public MusicDbContext(DbContextOptions<MusicDbContext> options) : base(options) { }

        public DbSet<Artist> Artists { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Genre> Genres { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Song>()
        .HasOne(s => s.Artist)
        .WithMany()
        .HasForeignKey(s => s.ArtistId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Song>()
        .HasOne(s => s.Genre)
        .WithMany()
        .HasForeignKey(s => s.GenreId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Song>()
        .HasOne(s => s.Album)
        .WithMany()
        .HasForeignKey(s => s.AlbumId)
        .OnDelete(DeleteBehavior.Restrict);
}
    }
    
}
