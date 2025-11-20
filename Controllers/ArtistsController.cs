using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopify.Data;
using Shopify.Models;

namespace Shopify.Controllers
{
    public class ArtistsController : Controller
    {
        private readonly MusicDbContext _context;

        public ArtistsController(MusicDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Detail(int id)
        {
            var artist = await _context.Artists
                .Include(a => a.Songs)
                .Include(a => a.Albums)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);

            if (artist == null)
                return NotFound();

            return View(artist);
        }
    }
}
