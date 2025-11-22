using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopify.Data;
using Shopify.Models;

namespace Shopify.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdvertisementController : Controller
    {
        private readonly MusicDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdvertisementController(MusicDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Admin/Advertisement
        public async Task<IActionResult> Index()
        {
            var advertisements = await _context.Advertisements.ToListAsync();
            return View(advertisements);
        }

        // GET: Admin/Advertisement/Create
        public IActionResult Create()
        {
            return View();
        }

        // Helper method to save files
        private async Task<string?> SaveFile(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0) return null;

            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", subFolder);
            Directory.CreateDirectory(uploadsFolder);
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return $"/uploads/{subFolder}/{uniqueFileName}";
        }

        // Helper method to delete files
        private void DeleteFile(string? url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var path = Path.Combine(_env.WebRootPath, url.TrimStart('/'));
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
        }


        // POST: Admin/Advertisement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Advertisement advertisement, IFormFile? imageFile, IFormFile? audioFile)
        {
            if (ModelState.IsValid)
            {
                // Lưu Poster/Image
                advertisement.PosterUrl = await SaveFile(imageFile, "advertisements/posters");

                // Lưu Audio
                advertisement.AudioUrl = await SaveFile(audioFile, "advertisements/audios");

                advertisement.CreatedAt = DateTime.Now;
                _context.Add(advertisement);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm quảng cáo thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(advertisement);
        }

        // GET: Admin/Advertisement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null) return NotFound();

            return View(advertisement);
        }

        // POST: Admin/Advertisement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Advertisement advertisement, IFormFile? imageFile, IFormFile? audioFile)
        {
            if (id != advertisement.AdvertisementId) return NotFound();

            // Lấy dữ liệu hiện tại (không theo dõi)
            var existing = await _context.Advertisements.AsNoTracking().FirstOrDefaultAsync(a => a.AdvertisementId == id);
            if (existing == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý Poster/Image
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        DeleteFile(existing.PosterUrl); // Xóa ảnh cũ
                        advertisement.PosterUrl = await SaveFile(imageFile, "advertisements/posters");
                    }
                    else
                    {
                        advertisement.PosterUrl = existing.PosterUrl; // Giữ nguyên ảnh cũ
                    }

                    // Xử lý Audio File
                    if (audioFile != null && audioFile.Length > 0)
                    {
                        DeleteFile(existing.AudioUrl); // Xóa file âm thanh cũ
                        advertisement.AudioUrl = await SaveFile(audioFile, "advertisements/audios");
                    }
                    else
                    {
                        advertisement.AudioUrl = existing.AudioUrl; // Giữ nguyên audio cũ
                    }

                    // Giữ nguyên ngày tạo
                    advertisement.CreatedAt = existing.CreatedAt;

                    _context.Update(advertisement);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Cập nhật quảng cáo thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Advertisements.Any(e => e.AdvertisementId == advertisement.AdvertisementId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(advertisement);
        }

        // POST: Admin/Advertisement/DeleteConfirmed/5 (Xóa)
        [HttpPost("Admin/Advertisement/DeleteConfirmed/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null)
                return Json(new { success = false, message = "Không tìm thấy quảng cáo" });

            // Xóa file ảnh và audio
            DeleteFile(advertisement.PosterUrl);
            DeleteFile(advertisement.AudioUrl);

            _context.Advertisements.Remove(advertisement);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // POST: Admin/Advertisement/ToggleActive
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null) return NotFound();

            advertisement.IsActive = !advertisement.IsActive;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = advertisement.IsActive });
        }
    }
}