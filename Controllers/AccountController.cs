using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopify.Data;
using Shopify.Models;
using System.Security.Claims;

namespace Shopify.Controllers
{
    public class AccountController : Controller
    {
        private readonly MusicDbContext _context;

        public AccountController(MusicDbContext context)
        {
            _context = context;
        }

        // ======== LOGIN ========

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password);
            if (user == null)
            {
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu!");
                return View();
            }

            var claims = new List<Claim>
    {
        new Claim("UserId", user.Id.ToString()), // 🔥 thêm dòng này
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role.ToString())
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            return RedirectToAction("Index", "Home");
        }


        // ======== REGISTER ========

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            // Kiểm tra email trùng
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("", "Email đã được sử dụng!");
                return View(user);
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // ======== LOGOUT ========
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpPost]
        // Chỉ bind các thuộc tính được gửi từ form: Username và Email
        public IActionResult Profile([Bind("Username", "Email")] User updatedUser)
        {
            // Lấy UserId từ Claims
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
                return Unauthorized(); // Hoặc xử lý lỗi khác

            var userId = int.Parse(userIdClaim.Value);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound();

            // Không cần kiểm tra ModelState.IsValid cho User model đầy đủ nữa
            // vì ta chỉ cần các trường được bind.
            // Nếu bạn muốn kiểm tra ModelState cho các trường được bind:
            // if (!ModelState.IsValid)
            // {
            //     // Nếu có lỗi, bạn phải gán lại user cũ để hiển thị trên View
            //     return View(user);
            // }

            // Cập nhật thông tin (chỉ Username được sửa trong form của bạn)
            user.Username = updatedUser.Username;

            // Email trong form là readonly, nhưng nếu bind được, nó sẽ ghi đè
            // Tùy theo logic bạn muốn, nếu email luôn cố định, bạn không cần bind Email.
            // user.Email = updatedUser.Email; 

            _context.Update(user);
            _context.SaveChanges();

            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Profile");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return View(user);
        }



        // === CHANGE PASSWORD ===
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound();

            if (user.PasswordHash != oldPassword)
            {
                ViewBag.Error = "Mật khẩu cũ không đúng!";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu mới và xác nhận không khớp!";
                return View();
            }

            user.PasswordHash = newPassword;
            _context.SaveChanges();

            ViewBag.Success = "Đổi mật khẩu thành công!";
            return View();
        }

    }
}
