using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopify.Data;
using Shopify.Models;

namespace Shopify.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ArtistController : Controller
    {
        private readonly MusicDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ArtistController(MusicDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Admin/Artist
        public async Task<IActionResult> Index()
        {
            var artists = await _context.Artists.ToListAsync();
            return View(artists);
        }

        // GET: Admin/Artist/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Artist/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Artist artist, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/artists");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    artist.ImageUrl = "/uploads/artists/" + uniqueFileName;
                }

                _context.Add(artist);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm ca sĩ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(artist);
        }

        // GET: Admin/Artist/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var artist = await _context.Artists.FindAsync(id);
            if (artist == null) return NotFound();

            return View(artist);
        }

        // POST: Admin/Artist/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Artist artist, IFormFile? imageFile)
        {
            if (id != artist.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Artists.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                    if (existing == null) return NotFound();

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/artists");
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

                        artist.ImageUrl = "/uploads/artists/" + uniqueFileName;
                    }
                    else
                    {
                        artist.ImageUrl = existing.ImageUrl; // giữ nguyên ảnh cũ
                    }

                    _context.Update(artist);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Cập nhật ca sĩ thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Artists.Any(e => e.Id == artist.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(artist);
        }

        // GET: Admin/Artist/Delete/5
        [HttpPost("Admin/Artist/DeleteConfirmed/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist == null)
                return Json(new { success = false, message = "Không tìm thấy ca sĩ" });

            // Xóa file ảnh nếu có
            if (!string.IsNullOrEmpty(artist.ImageUrl))
            {
                var path = Path.Combine(_env.WebRootPath, artist.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // POST: Admin/Artist/ToggleActive
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist == null) return NotFound();

            artist.IsActive = !artist.IsActive;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = artist.IsActive });
        }

    }
}
