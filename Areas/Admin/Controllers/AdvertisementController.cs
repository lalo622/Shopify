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

        // POST: Admin/Advertisement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Advertisement advertisement, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/advertisements");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    advertisement.ImageUrl = "/uploads/advertisements/" + uniqueFileName;
                }

                advertisement.CreatedAt = DateTime.Now; // Đảm bảo ngày tạo được thiết lập
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
        public async Task<IActionResult> Edit(int id, Advertisement advertisement, IFormFile? imageFile)
        {
            if (id != advertisement.AdvertisementId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Advertisements.AsNoTracking().FirstOrDefaultAsync(a => a.AdvertisementId == id);
                    if (existing == null) return NotFound();

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/advertisements");
                        Directory.CreateDirectory(uploadsFolder);
                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        // Xóa ảnh cũ
                        if (!string.IsNullOrEmpty(existing.ImageUrl))
                        {
                            var oldPath = Path.Combine(_env.WebRootPath, existing.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        advertisement.ImageUrl = "/uploads/advertisements/" + uniqueFileName;
                    }
                    else
                    {
                        advertisement.ImageUrl = existing.ImageUrl; // Giữ nguyên ảnh cũ
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

            // Xóa file ảnh nếu có
            if (!string.IsNullOrEmpty(advertisement.ImageUrl))
            {
                var path = Path.Combine(_env.WebRootPath, advertisement.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

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