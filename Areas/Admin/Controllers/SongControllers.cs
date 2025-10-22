using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopify.Data;
using Shopify.Models;

public class SongsController : Controller
{
    private readonly MusicDbContext _context;

    public SongsController(MusicDbContext context)
    {
        _context = context;
    }

    // GET: Songs
    public async Task<IActionResult> Index()
    {
        var songs = _context.Songs
            .Include(s => s.Artist)
            .Include(s => s.Genre)
            .Include(s => s.Album);
        return View(await songs.ToListAsync());
    }

    // GET: Songs/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var song = await _context.Songs
            .Include(s => s.Artist)
            .Include(s => s.Genre)
            .Include(s => s.Album)
            .FirstOrDefaultAsync(m => m.SongId == id);

        if (song == null) return NotFound();

        return View(song);
    }

    // GET: Songs/Create
    public IActionResult Create()
    {
        ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name");
        ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name");
        ViewData["AlbumId"] = new SelectList(_context.Albums, "Id", "Title");
        return View();
    }

    // POST: Songs/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("SongId,Title,Description,AudioUrl,Duration,ArtistId,GenreId,AlbumId")] Song song)
    {
        if (ModelState.IsValid)
        {
            _context.Add(song);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name", song.ArtistId);
        ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name", song.GenreId);
        ViewData["AlbumId"] = new SelectList(_context.Albums, "Id", "Title", song.AlbumId);
        return View(song);
    }

    // GET: Songs/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var song = await _context.Songs.FindAsync(id);
        if (song == null) return NotFound();

        ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name", song.ArtistId);
        ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name", song.GenreId);
        ViewData["AlbumId"] = new SelectList(_context.Albums, "Id", "Title", song.AlbumId);
        return View(song);
    }

    // POST: Songs/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("SongId,Title,Description,AudioUrl,Duration,ArtistId,GenreId,AlbumId")] Song song)
    {
        if (id != song.SongId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(song);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SongExists(song.SongId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name", song.ArtistId);
        ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name", song.GenreId);
        ViewData["AlbumId"] = new SelectList(_context.Albums, "Id", "Title", song.AlbumId);
        return View(song);
    }

    // GET: Songs/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var song = await _context.Songs
            .Include(s => s.Artist)
            .Include(s => s.Genre)
            .Include(s => s.Album)
            .FirstOrDefaultAsync(m => m.SongId == id);

        if (song == null) return NotFound();

        return View(song);
    }

    // POST: Songs/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var song = await _context.Songs.FindAsync(id);
        if (song != null)
        {
            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool SongExists(int id)
    {
        return _context.Songs.Any(e => e.SongId == id);
    }
}
