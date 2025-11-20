using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopify.Data;
using Shopify.Models;

namespace Shopify.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GenresController : Controller
    {
        private readonly MusicDbContext _context;

        public GenresController(MusicDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Genres
        public async Task<IActionResult> Index()
        {
            var genres = await _context.Genres
                .Include(g => g.Songs)
                .OrderBy(g => g.Name)
                .ToListAsync();

            return View(genres);
        }

        // GET: Admin/Genres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genre = await _context.Genres
                .Include(g => g.Songs)
                .ThenInclude(s => s.Artist)
                .FirstOrDefaultAsync(m => m.GenreId == id);

            if (genre == null)
            {
                return NotFound();
            }

            return View(genre);
        }

        // GET: Admin/Genres/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Genres/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Genre genre)
        {
            // Xóa validation cho navigation properties
            ModelState.Remove("Songs");

            // Kiểm tra trùng tên thể loại (case-insensitive)
            if (!string.IsNullOrWhiteSpace(genre.Name))
            {
                var nameToCheck = genre.Name.Trim().ToLower();
                var existingGenre = await _context.Genres
                    .FirstOrDefaultAsync(g => g.Name.ToLower().Trim() == nameToCheck);

                if (existingGenre != null)
                {
                    ModelState.AddModelError("Name", "Thể loại với tên này đã tồn tại!");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Trim dữ liệu trước khi lưu
                    genre.Name = genre.Name.Trim();
                    if (!string.IsNullOrWhiteSpace(genre.Description))
                    {
                        genre.Description = genre.Description.Trim();
                    }

                    _context.Add(genre);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm thể loại thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi lưu thể loại: " + ex.Message);
                }
            }

            return View(genre);
        }

        // GET: Admin/Genres/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound();
            }

            return View(genre);
        }

        // POST: Admin/Genres/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Genre genre)
        {
            if (id != genre.GenreId)
            {
                return NotFound();
            }

            // Xóa validation cho navigation properties
            ModelState.Remove("Songs");

            // Kiểm tra trùng tên thể loại (trừ chính nó)
            if (!string.IsNullOrWhiteSpace(genre.Name))
            {
                var nameToCheck = genre.Name.Trim().ToLower();
                var existingGenre = await _context.Genres
                    .FirstOrDefaultAsync(g => g.Name.ToLower().Trim() == nameToCheck 
                                           && g.GenreId != genre.GenreId);

                if (existingGenre != null)
                {
                    ModelState.AddModelError("Name", "Thể loại với tên này đã tồn tại!");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Trim dữ liệu trước khi lưu
                    genre.Name = genre.Name.Trim();
                    if (!string.IsNullOrWhiteSpace(genre.Description))
                    {
                        genre.Description = genre.Description.Trim();
                    }

                    _context.Update(genre);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thể loại thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GenreExists(genre.GenreId))
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

            return View(genre);
        }

        // POST: Admin/Genres/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var genre = await _context.Genres
                .Include(g => g.Songs)
                .FirstOrDefaultAsync(g => g.GenreId == id);

            if (genre == null)
            {
                return NotFound();
            }

            // Kiểm tra xem thể loại có bài hát nào không
            if (genre.Songs != null && genre.Songs.Any())
            {
                TempData["ErrorMessage"] = $"Không thể xóa thể loại '{genre.Name}' vì có {genre.Songs.Count} bài hát đang sử dụng!";
                return RedirectToAction(nameof(Delete), new { id = id });
            }

            try
            {
                _context.Genres.Remove(genre);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa thể loại thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa thể loại này vì đang có dữ liệu liên quan!";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private bool GenreExists(int id)
        {
            return _context.Genres.Any(e => e.GenreId == id);
        }
    }
}