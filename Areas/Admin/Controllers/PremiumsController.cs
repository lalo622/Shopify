using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopify.Data;
using Shopify.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Shopify.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PremiumsController : Controller
    {
        private readonly MusicDbContext _context;

        public PremiumsController(MusicDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Premiums
        public async Task<IActionResult> Index()
        {
            var premiums = await _context.Premiums
                .OrderByDescending(p => p.Price)
                .ToListAsync();

            return View(premiums);
        }

        // GET: Admin/Premiums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var premium = await _context.Premiums
                .FirstOrDefaultAsync(m => m.PremiumId == id);

            if (premium == null)
            {
                return NotFound();
            }

            return View(premium);
        }

        // GET: Admin/Premiums/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Premiums/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Premium premium)
        {
            ModelState.Remove("PremiumId");
            ModelState.Remove("CreatedAt");

            if (!string.IsNullOrWhiteSpace(premium.Name))
            {
                var nameToCheck = premium.Name.Trim().ToLower();
                var existingPremium = await _context.Premiums
                    .FirstOrDefaultAsync(p => p.Name.ToLower().Trim() == nameToCheck);

                if (existingPremium != null)
                {
                    ModelState.AddModelError("Name", "Gói Premium với tên này đã tồn tại!");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    premium.Name = premium.Name.Trim();
                    if (!string.IsNullOrWhiteSpace(premium.Description))
                    {
                        premium.Description = premium.Description.Trim();
                    }

                    _context.Add(premium);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm gói Premium thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi lưu gói Premium: " + ex.Message);
                }
            }

            return View(premium);
        }

        // GET: Admin/Premiums/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var premium = await _context.Premiums.FindAsync(id);
            if (premium == null)
            {
                return NotFound();
            }
            return View(premium);
        }

        // POST: Admin/Premiums/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Premium premium)
        {
            if (id != premium.PremiumId)
            {
                return NotFound();
            }

            // Xóa validation không cần thiết
            ModelState.Remove("CreatedAt");

            // Kiểm tra trùng tên (trừ chính nó)
            if (!string.IsNullOrWhiteSpace(premium.Name))
            {
                var nameToCheck = premium.Name.Trim().ToLower();
                var existingPremium = await _context.Premiums
                    .FirstOrDefaultAsync(p => p.Name.ToLower().Trim() == nameToCheck && p.PremiumId != id);

                if (existingPremium != null)
                {
                    ModelState.AddModelError("Name", "Gói Premium với tên này đã tồn tại!");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Trim dữ liệu trước khi lưu
                    premium.Name = premium.Name.Trim();
                    if (!string.IsNullOrWhiteSpace(premium.Description))
                    {
                        premium.Description = premium.Description.Trim();
                    }

                    _context.Update(premium);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật gói Premium thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PremiumExists(premium.PremiumId))
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
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật gói Premium: " + ex.Message);
                }
            }
            return View(premium);
        }

        // GET: Admin/Premiums/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var premium = await _context.Premiums
                .FirstOrDefaultAsync(m => m.PremiumId == id);
            if (premium == null)
            {
                return NotFound();
            }

            return View(premium);
        }

        // POST: Admin/Premiums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var premium = await _context.Premiums.FindAsync(id);
            if (premium != null)
            {
                _context.Premiums.Remove(premium);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa gói Premium thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PremiumExists(int id)
        {
            return _context.Premiums.Any(e => e.PremiumId == id);
        }
    }
}