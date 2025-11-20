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
        public async Task<IActionResult> Create(
            [Bind("Title,Description,Duration,ArtistId,GenreId,AlbumId")] Song song,
            IFormFile audioFile,
            IFormFile imageFile)
        {
            Console.WriteLine($"Title: {song?.Title ?? "NULL"}");
            Console.WriteLine($"ArtistId: {song?.ArtistId}");
            Console.WriteLine($"GenreId: {song?.GenreId}");
            Console.WriteLine($"AudioFile: {audioFile?.FileName ?? "NULL"}");
            Console.WriteLine($"ImageFile: {imageFile?.FileName ?? "NULL"}");

            // Xóa validation cho các navigation properties
            ModelState.Remove("Artist");
            ModelState.Remove("Genre");
            ModelState.Remove("Album");
            ModelState.Remove("AudioUrl");
            ModelState.Remove("ImageUrl");

            // Kiểm tra trùng tên bài hát (case-insensitive)
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
                // Kiểm tra định dạng file nhạc
                var allowedAudioExtensions = new[] { ".mp3", ".wav", ".m4a", ".aac", ".ogg" };
                var audioFileExtension = Path.GetExtension(audioFile.FileName).ToLower();

                if (!allowedAudioExtensions.Contains(audioFileExtension))
                {
                    ModelState.AddModelError("AudioFile", "Chỉ chấp nhận file nhạc (MP3, WAV, M4A, AAC, OGG)!");
                }

                // Kiểm tra kích thước file nhạc (tối đa 50MB)
                if (audioFile.Length > 50 * 1024 * 1024)
                {
                    ModelState.AddModelError("AudioFile", "Kích thước file nhạc không được vượt quá 50MB!");
                }
            }

            // Validate file ảnh nếu có
            if (imageFile != null && imageFile.Length > 0)
            {
                // Kiểm tra định dạng file ảnh
                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                var imageFileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                if (!allowedImageExtensions.Contains(imageFileExtension))
                {
                    ModelState.AddModelError("ImageFile", "Chỉ chấp nhận file ảnh (JPG, JPEG, PNG, GIF, BMP, WEBP)!");
                }

                // Kiểm tra kích thước file ảnh (tối đa 10MB)
                if (imageFile.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Kích thước file ảnh không được vượt quá 10MB!");
                }
            }

            // QUAN TRỌNG: Kiểm tra ArtistId và GenreId có hợp lệ không
            if (song.ArtistId <= 0)
            {
                ModelState.AddModelError("ArtistId", "Vui lòng chọn nghệ sĩ!");
            }

            if (song.GenreId <= 0)
            {
                ModelState.AddModelError("GenreId", "Vui lòng chọn thể loại!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Upload file nhạc
                    if (audioFile != null && audioFile.Length > 0)
                    {
                        string audioUploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "songs");

                        // Tạo thư mục nếu chưa tồn tại
                        if (!Directory.Exists(audioUploadsFolder))
                        {
                            Directory.CreateDirectory(audioUploadsFolder);
                        }

                        // Tạo tên file unique cho audio
                        string uniqueAudioFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(audioFile.FileName);
                        string audioFilePath = Path.Combine(audioUploadsFolder, uniqueAudioFileName);

                        // Lưu file nhạc
                        using (var fileStream = new FileStream(audioFilePath, FileMode.Create))
                        {
                            await audioFile.CopyToAsync(fileStream);
                        }

                        // Lưu đường dẫn tương đối vào database
                        song.AudioUrl = "/uploads/songs/" + uniqueAudioFileName;
                    }

                    // Upload file ảnh
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string imageUploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "images", "songs");

                        // Tạo thư mục nếu chưa tồn tại
                        if (!Directory.Exists(imageUploadsFolder))
                        {
                            Directory.CreateDirectory(imageUploadsFolder);
                        }

                        // Tạo tên file unique cho ảnh
                        string uniqueImageFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        string imageFilePath = Path.Combine(imageUploadsFolder, uniqueImageFileName);

                        // Lưu file ảnh
                        using (var fileStream = new FileStream(imageFilePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        // Lưu đường dẫn tương đối vào database
                        song.ImageUrl = "/uploads/images/songs/" + uniqueImageFileName;
                    }

                    _context.Add(song);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm bài hát thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Lỗi: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi lưu bài hát: " + ex.Message);
                }
            }
            else
            {
                // Log tất cả lỗi ModelState
                Console.WriteLine("=== ModelState Errors ===");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Count > 0)
                    {
                        Console.WriteLine($"Key: {key}");
                        foreach (var error in state.Errors)
                        {
                            Console.WriteLine($"  - {error.ErrorMessage}");
                        }
                    }
                }
            }

            // Nếu có lỗi, load lại danh sách
            ViewBag.Artists = await _context.Artists.OrderBy(a => a.Name).ToListAsync();
            ViewBag.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            ViewBag.Albums = await _context.Albums.Include(a => a.Artist).OrderBy(a => a.Title).ToListAsync();
            return View(song);
        }

        // GET: Admin/Songs/Edit/
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var song = await _context.Songs.FindAsync(id);
            if (song == null)
                return NotFound();

            ViewBag.Artists = await _context.Artists.OrderBy(a => a.Name).ToListAsync();
            ViewBag.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            ViewBag.Albums = await _context.Albums.Include(a => a.Artist).OrderBy(a => a.Title).ToListAsync();
            return View(song);
        }

        // POST: Admin/Songs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("SongId,Title,Description,Duration,ArtistId,GenreId,AlbumId")] Song song,
            IFormFile? audioFile,
            IFormFile? imageFile)
        {
            if (id != song.SongId)
                return NotFound();

            var existingEntity = await _context.Songs.FindAsync(id);
            if (existingEntity == null)
                return NotFound();

            // Remove navigation properties from ModelState
            ModelState.Remove("Artist");
            ModelState.Remove("Genre");
            ModelState.Remove("Album");
            ModelState.Remove("AudioUrl");
            ModelState.Remove("ImageUrl");

            // Nếu client không gửi Title, giữ nguyên
            if (string.IsNullOrWhiteSpace(song.Title))
            {
                song.Title = existingEntity.Title;
                ModelState.Remove(nameof(song.Title));
            }

            // Kiểm tra trùng tên (ngoại trừ chính nó)
            if (!string.IsNullOrWhiteSpace(song.Title))
            {
                bool exists = await _context.Songs
                    .AnyAsync(s => s.Title.ToLower().Trim() == song.Title.Trim().ToLower() && s.SongId != id);
                if (exists) ModelState.AddModelError("Title", "Bài hát với tên này đã tồn tại!");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Artists = await _context.Artists.OrderBy(a => a.Name).ToListAsync();
                ViewBag.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                ViewBag.Albums = await _context.Albums.Include(a => a.Artist).OrderBy(a => a.Title).ToListAsync();
                return View(song);
            }

            // Cập nhật dữ liệu
            existingEntity.Title = song.Title;
            existingEntity.Description = song.Description;
            existingEntity.ArtistId = song.ArtistId;
            existingEntity.GenreId = song.GenreId;
            existingEntity.AlbumId = song.AlbumId;
            existingEntity.Duration = song.Duration;

            // Xử lý audioFile mới
            if (audioFile != null && audioFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingEntity.AudioUrl))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, existingEntity.AudioUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                string fileName = Guid.NewGuid() + Path.GetExtension(audioFile.FileName);
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "songs");
                Directory.CreateDirectory(uploadFolder);
                string fullPath = Path.Combine(uploadFolder, fileName);
                using var stream = new FileStream(fullPath, FileMode.Create);
                await audioFile.CopyToAsync(stream);
                existingEntity.AudioUrl = "/uploads/songs/" + fileName;
            }

            // Xử lý imageFile mới
            if (imageFile != null && imageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingEntity.ImageUrl))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, existingEntity.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "images", "songs");
                Directory.CreateDirectory(uploadFolder);
                string fullPath = Path.Combine(uploadFolder, fileName);
                using var stream = new FileStream(fullPath, FileMode.Create);
                await imageFile.CopyToAsync(stream);
                existingEntity.ImageUrl = "/uploads/images/songs/" + fileName;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật bài hát thành công!";

            return RedirectToAction(nameof(Edit), new { id });
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
                    string audioFilePath = Path.Combine(_webHostEnvironment.WebRootPath, song.AudioUrl.TrimStart('/'));
                    if (System.IO.File.Exists(audioFilePath))
                    {
                        System.IO.File.Delete(audioFilePath);
                    }
                }

                // Xóa file ảnh nếu tồn tại
                if (!string.IsNullOrEmpty(song.ImageUrl))
                {
                    string imageFilePath = Path.Combine(_webHostEnvironment.WebRootPath, song.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imageFilePath))
                    {
                        System.IO.File.Delete(imageFilePath);
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
        [HttpPost]
        public async Task<IActionResult> ToggleVip(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
                return NotFound();

            song.IsVip = !song.IsVip; // Đảo trạng thái
            await _context.SaveChangesAsync();

            // Trả JSON để dùng AJAX toggle mà không reload trang
            return Json(new { success = true, isVip = song.IsVip });
        }
    }
}