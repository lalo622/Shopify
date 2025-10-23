using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopify.Data;
using Shopify.Models;

namespace Shopify.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SongsController : Controller
    {
        private readonly MusicDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SongsController(MusicDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Songs
        public async Task<IActionResult> Index()
        {
            var songs = _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Genre);
            return View(await songs.ToListAsync());
        }

        // GET: Songs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var song = await _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Genre)
                .FirstOrDefaultAsync(m => m.SongId == id);

            if (song == null) return NotFound();

            return View(song);
        }

        // GET: Songs/Create
        public IActionResult Create()
        {
            ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name");
            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name");
            return View();
        }

        // POST: Songs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SongId,Title,Description,Duration,ArtistId,GenreId")] Song song, IFormFile audioFile)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload file nhạc
                if (audioFile != null && audioFile.Length > 0)
                {
                    // Kiểm tra định dạng file
                    var allowedExtensions = new[] { ".mp3", ".wav", ".m4a", ".aac" };
                    var fileExtension = Path.GetExtension(audioFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("AudioFile", "Chỉ chấp nhận file nhạc (mp3, wav, m4a, aac)");
                        ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name", song.ArtistId);
                        ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name", song.GenreId);
                        return View(song);
                    }

                    // Tạo tên file unique với timestamp để tránh trùng
                    var fileName = $"{Guid.NewGuid()}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "songs");

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Kiểm tra xem file đã tồn tại chưa (phòng trường hợp hiếm)
                    if (System.IO.File.Exists(filePath))
                    {
                        // Nếu trùng, tạo tên mới
                        fileName = $"{Guid.NewGuid()}_{DateTime.Now:yyyyMMddHHmmssfff}{fileExtension}";
                        filePath = Path.Combine(uploadsFolder, fileName);
                    }

                    try
                    {
                        // Lưu file
                        using (var stream = new FileStream(filePath, FileMode.CreateNew)) // Dùng CreateNew để tránh ghi đè
                        {
                            await audioFile.CopyToAsync(stream);
                        }

                        // Lưu đường dẫn file vào database
                        song.AudioUrl = "/uploads/songs/" + fileName;
                    }
                    catch (IOException ex) when (ex.Message.Contains("already exists"))
                    {
                        ModelState.AddModelError("AudioFile", "File đã tồn tại. Vui lòng thử lại.");
                        ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name", song.ArtistId);
                        ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name", song.GenreId);
                        return View(song);
                    }
                }
                else
                {
                    ModelState.AddModelError("AudioFile", "Vui lòng chọn file nhạc");
                    ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name", song.ArtistId);
                    ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name", song.GenreId);
                    return View(song);
                }

                _context.Add(song);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name", song.ArtistId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name", song.GenreId);
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
            return View(song);
        }

        // POST: Songs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SongId,Title,Description,AudioUrl,Duration,ArtistId,GenreId")] Song song, IFormFile audioFile)
        {
            if (id != song.SongId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload file nhạc mới nếu có
                    if (audioFile != null && audioFile.Length > 0)
                    {
                        // Kiểm tra định dạng file
                        var allowedExtensions = new[] { ".mp3", ".wav", ".m4a", ".aac" };
                        var fileExtension = Path.GetExtension(audioFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("AudioFile", "Chỉ chấp nhận file nhạc (mp3, wav, m4a, aac)");
                            ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name", song.ArtistId);
                            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name", song.GenreId);
                            return View(song);
                        }

                        // Tạo tên file mới với timestamp
                        var fileName = $"{Guid.NewGuid()}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "songs");

                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Kiểm tra trùng tên
                        if (System.IO.File.Exists(filePath))
                        {
                            fileName = $"{Guid.NewGuid()}_{DateTime.Now:yyyyMMddHHmmssfff}{fileExtension}";
                            filePath = Path.Combine(uploadsFolder, fileName);
                        }

                        // Lưu file mới
                        using (var stream = new FileStream(filePath, FileMode.CreateNew))
                        {
                            await audioFile.CopyToAsync(stream);
                        }

                        // Xóa file cũ nếu tồn tại
                        if (!string.IsNullOrEmpty(song.AudioUrl))
                        {
                            var oldFilePath = Path.Combine(_environment.WebRootPath, song.AudioUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Cập nhật đường dẫn file mới
                        song.AudioUrl = "/uploads/songs/" + fileName;
                    }

                    _context.Update(song);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SongExists(song.SongId)) return NotFound();
                    else throw;
                }
                catch (IOException ex) when (ex.Message.Contains("already exists"))
                {
                    ModelState.AddModelError("AudioFile", "File đã tồn tại. Vui lòng thử lại.");
                    ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name", song.ArtistId);
                    ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name", song.GenreId);
                    return View(song);
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ArtistId"] = new SelectList(_context.Artists, "ArtistId", "Name", song.ArtistId);
            ViewData["GenreId"] = new SelectList(_context.Genres, "GenreId", "Name", song.GenreId);
            return View(song);
        }

        // GET: Songs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var song = await _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Genre)
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
                // Xóa file nhạc khi xóa bài hát
                if (!string.IsNullOrEmpty(song.AudioUrl))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, song.AudioUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

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
}