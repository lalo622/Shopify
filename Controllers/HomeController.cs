using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopify.Data;
using Shopify.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Shopify.Controllers
{
    public class HomeController : Controller
    {
        private readonly MusicDbContext _context;

        public HomeController(MusicDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel
            {
                RecentSongs = await _context.Songs
                    .Include(s => s.Artist)
                    .Include(s => s.Album)
                    .OrderByDescending(s => s.SongId)
                    .Take(6)
                    .ToListAsync(),

                TrendingSongs = await _context.Songs
                    .Include(s => s.Artist)
                    .OrderBy(s => s.SongId)
                    .Take(6)
                    .ToListAsync(),

                PopularArtists = await _context.Artists
                    .Take(4)
                    .ToListAsync(),

                PopularAlbums = await _context.Albums
                    .Include(a => a.Artist)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
