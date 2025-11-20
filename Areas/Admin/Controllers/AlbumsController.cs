using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopify.Data;
using Shopify.Models;

namespace Shopify.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AlbumsController : Controller
    {
        private readonly MusicDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AlbumsController(MusicDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin/Albums
        public async Task<IActionResult> Index()
        {
            var albums = await _context.Albums
                .Include(a => a.Artist)
                .OrderByDescending(a => a.ReleaseDate)
                .ToListAsync();
            return View(albums);
        }

        // GET: Admin/Albums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(m => m.AlbumId == id);

            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // GET: Admin/Albums/Create
        public async Task<IActionResult> Create()
        {
            ViewData["ArtistId"] = new SelectList(_context.Artists.Where(a => a.IsActive), "Id", "Name");
            ViewData["Songs"] = new SelectList(await _context.Songs.ToListAsync(), "SongId", "Title");
            return View();
        }

        // POST: Admin/Albums/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Album album, IFormFile? ImageFile, List<int>? SelectedSongs)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    album.CoverImageUrl = await SaveImage(ImageFile);
                }

                _context.Add(album);
                await _context.SaveChangesAsync();

                // Thêm các bài hát vào album
                if (SelectedSongs != null && SelectedSongs.Any())
                {
                    var songs = await _context.Songs.Where(s => SelectedSongs.Contains(s.SongId)).ToListAsync();
                    foreach (var song in songs)
                    {
                        song.AlbumId = album.AlbumId;
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Thêm album thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["ArtistId"] = new SelectList(_context.Artists.Where(a => a.IsActive), "Id", "Name", album.ArtistId);
            ViewData["Songs"] = new SelectList(await _context.Songs.ToListAsync(), "SongId", "Title");
            return View(album);
        }

        // GET: Admin/Albums/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(a => a.AlbumId == id);

            if (album == null)
            {
                return NotFound();
            }

            ViewData["ArtistId"] = new SelectList(_context.Artists.Where(a => a.IsActive), "Id", "Name", album.ArtistId);
            ViewData["Songs"] = new SelectList(await _context.Songs.ToListAsync(), "SongId", "Title");
            ViewData["SelectedSongs"] = album.Songs?.Select(s => s.SongId).ToList() ?? new List<int>();
            return View(album);
        }

        // POST: Admin/Albums/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Album album, IFormFile? ImageFile, List<int>? SelectedSongs)
        {
            if (id != album.AlbumId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAlbum = await _context.Albums
                        .Include(a => a.Songs)
                        .FirstOrDefaultAsync(a => a.AlbumId == id);

                    if (existingAlbum == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các thuộc tính
                    existingAlbum.Title = album.Title;
                    existingAlbum.ReleaseDate = album.ReleaseDate;
                    existingAlbum.ArtistId = album.ArtistId;

                    // Xử lý upload ảnh mới
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(existingAlbum.CoverImageUrl))
                        {
                            DeleteImage(existingAlbum.CoverImageUrl);
                        }
                        existingAlbum.CoverImageUrl = await SaveImage(ImageFile);
                    }

                    // Cập nhật danh sách bài hát
                    // Xóa tất cả bài hát cũ
                    var oldSongs = await _context.Songs.Where(s => s.AlbumId == id).ToListAsync();
                    foreach (var song in oldSongs)
                    {
                        song.AlbumId = null;
                    }

                    // Thêm bài hát mới
                    if (SelectedSongs != null && SelectedSongs.Any())
                    {
                        var newSongs = await _context.Songs.Where(s => SelectedSongs.Contains(s.SongId)).ToListAsync();
                        foreach (var song in newSongs)
                        {
                            song.AlbumId = existingAlbum.AlbumId;
                        }
                    }

                    _context.Update(existingAlbum);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật album thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlbumExists(album.AlbumId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ArtistId"] = new SelectList(_context.Artists.Where(a => a.IsActive), "Id", "Name", album.ArtistId);
            ViewData["Songs"] = new SelectList(await _context.Songs.ToListAsync(), "SongId", "Title");
            ViewData["SelectedSongs"] = SelectedSongs ?? new List<int>();
            return View(album);
        }

        // GET: Admin/Albums/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(m => m.AlbumId == id);

            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // POST: Admin/Albums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var album = await _context.Albums
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(a => a.AlbumId == id);

            if (album != null)
            {
                // Xóa ảnh nếu có
                if (!string.IsNullOrEmpty(album.CoverImageUrl))
                {
                    DeleteImage(album.CoverImageUrl);
                }

                // Gỡ liên kết bài hát với album
                if (album.Songs != null)
                {
                    foreach (var song in album.Songs)
                    {
                        song.AlbumId = null;
                    }
                }

                _context.Albums.Remove(album);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa album thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AlbumExists(int id)
        {
            return _context.Albums.Any(e => e.AlbumId == id);
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "albums");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/images/albums/{uniqueFileName}";
        }

        private void DeleteImage(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }
    }
}