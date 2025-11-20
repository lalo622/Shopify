using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopify.Data; // Đảm bảo đúng DbContext
using Shopify.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Shopify.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PremiumsController : Controller
    {
        private readonly MusicDbContext _context; // Đã thay đổi DbContext

        public PremiumsController(MusicDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Premiums
        public async Task<IActionResult> Index()
        {
            var premiums = await _context.Premiums
                .OrderByDescending(p => p.Price) // Sắp xếp theo giá
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
            // THÊM DÒNG NÀY - Xóa validation cho navigation properties và các field không cần thiết
            ModelState.Remove("PremiumId");
            ModelState.Remove("CreatedAt");

            // Kiểm tra trùng tên gói VIP (case-insensitive)
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
                    // Trim dữ liệu trước khi lưu
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

        // --- Các Action Edit, Delete và GenreExists đã được bỏ qua để tập trung vào Create ---

        private bool PremiumExists(int id)
        {
            return _context.Premiums.Any(e => e.PremiumId == id);
        }
    }
}