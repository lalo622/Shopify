using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopify.Data;
using Shopify.Models;

public class PlaylistController : Controller
{
    private readonly MusicDbContext _context;

    public PlaylistController(MusicDbContext context)
    {
        _context = context;
    }

    
    public async Task<IActionResult> Index()
    {
        int userId = 1; 

        var playlists = await _context.Playlists
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return View(playlists);
    }

    
    public IActionResult Create()
    {
        return View();
    }

    
    [HttpPost]
    public async Task<IActionResult> Create(Playlist playlist)
    {
        int userId = 1;

        playlist.UserId = userId;
        playlist.CreatedAt = DateTime.Now;

        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

   
    public async Task<IActionResult> Details(int id)
    {
        var playlist = await _context.Playlists
            .Include(p => p.PlaylistSongs)
            .ThenInclude(ps => ps.Song)
            .FirstOrDefaultAsync(p => p.PlaylistId == id);

        if (playlist == null)
            return NotFound();

        ViewBag.AllSongs = await _context.Songs.ToListAsync();

        return View(playlist);
    }

   
    [HttpPost]
    public async Task<IActionResult> AddSong(int playlistId, int songId)
    {
        var exists = await _context.PlaylistSongs
            .AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

        if (!exists)
        {
            _context.PlaylistSongs.Add(new PlaylistSong
            {
                PlaylistId = playlistId,
                SongId = songId
            });

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", new { id = playlistId });
    }
    public async Task<IActionResult> Delete(int id)
    {
        var playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.PlaylistId == id);
        if (playlist == null)
            return NotFound();

        return View(playlist);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var playlist = await _context.Playlists
            .Include(p => p.PlaylistSongs)
            .FirstOrDefaultAsync(p => p.PlaylistId == id);

        if (playlist == null)
            return NotFound();

     
        _context.PlaylistSongs.RemoveRange(playlist.PlaylistSongs);

        _context.Playlists.Remove(playlist);

        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}
