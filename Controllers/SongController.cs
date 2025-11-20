using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopify.Data;
using Shopify.Models.ViewModels;
using System.Threading.Tasks;

namespace Shopify.Controllers
{
    public class SongController : Controller
    {
        private readonly MusicDbContext _context;

        public SongController(MusicDbContext context)
        {
            _context = context;
        }

        // GET: /Song/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var song = await _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Genre)
                .Include(s => s.Album)
                .FirstOrDefaultAsync(s => s.SongId == id);

            if (song == null)
            {
                return NotFound();
            }

            // Tăng lượt nghe
            song.PlayCount += 1;
            await _context.SaveChangesAsync();

            var viewModel = new SongDetailViewModel
            {
                SongId = song.SongId,
                Title = song.Title,
                Description = song.Description,
                AudioUrl = song.AudioUrl,
                Duration = song.Duration,
                ImageUrl = song.ImageUrl,
                IsVip = song.IsVip,
                PlayCount = song.PlayCount,
                ArtistName = song.Artist?.Name ?? "Unknown Artist",
                GenreName = song.Genre?.Name ?? "Unknown Genre",
                AlbumTitle = song.Album?.Title
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> IncreasePlayCount(int id)
        {
            var song = await _context.Songs.FirstOrDefaultAsync(s => s.SongId == id);
            if (song == null)
                return NotFound();

            song.PlayCount += 1;
            await _context.SaveChangesAsync();

            return Ok(new { playCount = song.PlayCount });
        }

    }
}
