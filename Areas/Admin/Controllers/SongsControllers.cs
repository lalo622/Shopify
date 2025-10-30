using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopify.Data;
using Shopify.Models;
using Microsoft.AspNetCore.Hosting;

namespace Shopify.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SongsController : Controller
    {
        private readonly MusicDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SongsController(MusicDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Songs
        public async Task<IActionResult> Index()
        {
            var songs = await _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Genre)
                .Include(s => s.Album)
                .OrderByDescending(s => s.SongId)
                .ToListAsync();

            return View(songs);
        }

        // GET: Admin/Songs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var song = await _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Genre)
                .Include(s => s.Album)
                .FirstOrDefaultAsync(m => m.SongId == id);

            if (song == null)
            {
                return NotFound();
            }

            return View(song);
        }

        // GET: Admin/Songs/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Artists = await _context.Artists.OrderBy(a => a.Name).ToListAsync();
            ViewBag.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            ViewBag.Albums = await _context.Albums.Include(a => a.Artist).OrderBy(a => a.Title).ToListAsync();
            return View();
        }

        // POST: Admin/Songs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Create(Song song, IFormFile audioFile)
        {
            Console.WriteLine($"Title: {song?.Title ?? "NULL"}");
            Console.WriteLine($"ArtistId: {song?.ArtistId}");
            Console.WriteLine($"GenreId: {song?.GenreId}");
            Console.WriteLine($"AudioFile: {audioFile?.FileName ?? "NULL"}");
            // Xóa validation cho các navigation properties
            ModelState.Remove("Artist");
            ModelState.Remove("Genre");
            ModelState.Remove("Album");
            ModelState.Remove("AudioUrl");

            // Kiểm tra trùng tên bài hát (case-insensitive) - FIX NULL
            if (!string.IsNullOrWhiteSpace(song.Title))
            {
                var titleToCheck = song.Title.Trim().ToLower();
                var existingSong = await _context.Songs
                    .FirstOrDefaultAsync(s => s.Title.ToLower().Trim() == titleToCheck);

                if (existingSong != null)
                {
                    ModelState.AddModelError("Title", "Bài hát với tên này đã tồn tại!");
                }
            }

            // Validate file nhạc
            if (audioFile == null || audioFile.Length == 0)
            {
                ModelState.AddModelError("AudioFile", "Vui lòng chọn file nhạc!");
            }
            else
            {
                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".mp3", ".wav", ".m4a", ".aac", ".ogg" };
                var fileExtension = Path.GetExtension(audioFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("AudioFile", "Chỉ chấp nhận file nhạc (MP3, WAV, M4A, AAC, OGG)!");
                }

                // Kiểm tra kích thước file (tối đa 50MB)
                if (audioFile.Length > 50 * 1024 * 1024)
                {
                    ModelState.AddModelError("AudioFile", "Kích thước file không được vượt quá 50MB!");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Upload file nhạc
                    if (audioFile != null && audioFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "songs");
                        
                        // Tạo thư mục nếu chưa tồn tại
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Tạo tên file unique
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(audioFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Lưu file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await audioFile.CopyToAsync(fileStream);
                        }

                        // Lưu đường dẫn tương đối vào database
                        song.AudioUrl = "/uploads/songs/" + uniqueFileName;
                    }

                    _context.Add(song);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm bài hát thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi lưu bài hát: " + ex.Message);
                }
            }

            // Nếu có lỗi, load lại danh sách
            ViewBag.Artists = await _context.Artists.OrderBy(a => a.Name).ToListAsync();
            ViewBag.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            ViewBag.Albums = await _context.Albums.Include(a => a.Artist).OrderBy(a => a.Title).ToListAsync();
            return View(song);
        }

        // GET: Admin/Songs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var song = await _context.Songs.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            ViewBag.Artists = await _context.Artists.OrderBy(a => a.Name).ToListAsync();
            ViewBag.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            ViewBag.Albums = await _context.Albums.Include(a => a.Artist).OrderBy(a => a.Title).ToListAsync();
            return View(song);
        }

        // POST: Admin/Songs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Song song, IFormFile? audioFile)
        {
            if (id != song.SongId)
            {
                return NotFound();
            }

            // Xóa validation cho các navigation properties
            ModelState.Remove("Artist");
            ModelState.Remove("Genre");
            ModelState.Remove("Album");
            ModelState.Remove("AudioUrl");

            // Kiểm tra trùng tên bài hát (trừ chính nó) - FIX NULL
            if (!string.IsNullOrWhiteSpace(song.Title))
            {
                var titleToCheck = song.Title.Trim().ToLower();
                var existingSong = await _context.Songs
                    .FirstOrDefaultAsync(s => s.Title.ToLower().Trim() == titleToCheck 
                                           && s.SongId != song.SongId);

                if (existingSong != null)
                {
                    ModelState.AddModelError("Title", "Bài hát với tên này đã tồn tại!");
                }
            }

            // Validate file nhạc nếu có upload file mới
            if (audioFile != null && audioFile.Length > 0)
            {
                var allowedExtensions = new[] { ".mp3", ".wav", ".m4a", ".aac", ".ogg" };
                var fileExtension = Path.GetExtension(audioFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("AudioFile", "Chỉ chấp nhận file nhạc (MP3, WAV, M4A, AAC, OGG)!");
                }

                if (audioFile.Length > 50 * 1024 * 1024)
                {
                    ModelState.AddModelError("AudioFile", "Kích thước file không được vượt quá 50MB!");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEntity = await _context.Songs.AsNoTracking().FirstOrDefaultAsync(s => s.SongId == id);
                    if (existingEntity == null)
                    {
                        return NotFound();
                    }

                    // Giữ lại AudioUrl cũ nếu không upload file mới
                    string oldAudioUrl = existingEntity.AudioUrl;
                    song.AudioUrl = oldAudioUrl;

                    // Nếu có upload file mới
                    if (audioFile != null && audioFile.Length > 0)
                    {
                        // Xóa file cũ nếu tồn tại
                        if (!string.IsNullOrEmpty(oldAudioUrl))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, oldAudioUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Upload file mới
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "songs");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(audioFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await audioFile.CopyToAsync(fileStream);
                        }

                        song.AudioUrl = "/uploads/songs/" + uniqueFileName;
                    }

                    _context.Update(song);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật bài hát thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SongExists(song.SongId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật: " + ex.Message);
                }
            }

            ViewBag.Artists = await _context.Artists.OrderBy(a => a.Name).ToListAsync();
            ViewBag.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            ViewBag.Albums = await _context.Albums.Include(a => a.Artist).OrderBy(a => a.Title).ToListAsync();
            return View(song);
        }

        // GET: Admin/Songs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var song = await _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Genre)
                .Include(s => s.Album)
                .FirstOrDefaultAsync(m => m.SongId == id);

            if (song == null)
            {
                return NotFound();
            }

            return View(song);
        }

        // POST: Admin/Songs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            
            if (song == null)
            {
                return NotFound();
            }

            try
            {
                // Xóa file nhạc nếu tồn tại
                if (!string.IsNullOrEmpty(song.AudioUrl))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, song.AudioUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Songs.Remove(song);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa bài hát thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Xử lý lỗi khi bài hát có ràng buộc với dữ liệu khác
                TempData["ErrorMessage"] = "Không thể xóa bài hát này vì đang có dữ liệu liên quan!";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private bool SongExists(int id)
        {
            return _context.Songs.Any(e => e.SongId == id);
        }
    }
}