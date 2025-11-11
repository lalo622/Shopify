using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using Shopify.Data;
using Shopify.Models;
using Shopify.Service;
using BCrypt.Net;

namespace Shopify.Controllers
{
    public class AccountController : Controller
    {
        private readonly MusicDbContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        private static readonly ConcurrentDictionary<string, (string Otp, DateTime Expiry)> _otpCache = new();

        public AccountController(MusicDbContext context, IConfiguration config, EmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        // ========================= View Models =========================

        public class RegisterViewModel
        {
            [Required(ErrorMessage = "Tên người dùng là bắt buộc.")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email là bắt buộc.")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "OTP là bắt buộc.")]
            public string Otp { get; set; } = string.Empty;

            public string OtpStatus { get; set; } = string.Empty;
        }

        public class LoginViewModel
        {
            [Required(ErrorMessage = "Email là bắt buộc.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
            public string Password { get; set; } = string.Empty;
        }

        public class ChangePasswordViewModel
        {
            [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc.")]
            [DataType(DataType.Password)]
            public string OldPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public class ForgotPasswordViewModel
        {
            [Required(ErrorMessage = "Email là bắt buộc.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mã OTP là bắt buộc.")]
            public string Otp { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            public string OtpStatus { get; set; } = string.Empty;
        }

        // ========================= LOGIN =========================

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu!");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Role, user.Role ?? "Member")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );

            return RedirectToAction("Index", "Home");
        }

        // ========================= REGISTER =========================

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        // Bước 1: Gửi OTP
        [HttpPost]
        [ActionName("Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendOtpRegister(RegisterViewModel model)
        {
            // Kiểm tra Email
            if (string.IsNullOrWhiteSpace(model.Email) || !new EmailAddressAttribute().IsValid(model.Email))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ.");
                return View(model);
            }

            // Xóa validation của các trường khác (Username, Password, Otp)
            foreach (var key in ModelState.Keys.Where(k => k != nameof(RegisterViewModel.Email)).ToList())
            {
                ModelState.Remove(key);
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký.");
                return View(model);
            }

            try
            {
                var otp = new Random().Next(100000, 999999).ToString();
                _otpCache[model.Email] = (otp, DateTime.Now.AddMinutes(5));

                await _emailService.SendEmailAsync(model.Email, "Mã OTP đăng ký", $"Mã OTP của bạn là: <b>{otp}</b>");

                model.OtpStatus = "OTP_SENT";
                ViewBag.Success = "Mã OTP đã được gửi tới email của bạn. Mã có hiệu lực trong 5 phút.";
                return View(model);
            }
            catch
            {
                ViewBag.Error = "Lỗi khi gửi email. Vui lòng thử lại.";
                return View(model);
            }
        }

        // Bước 2: Xác minh OTP & Đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAndRegister(RegisterViewModel model)
        {
            // Kiểm tra các trường cần thiết cho bước 2
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.Otp))
            {
                ViewBag.Error = "Vui lòng điền đầy đủ Tên người dùng, Mật khẩu và Mã OTP.";
                model.OtpStatus = "OTP_SENT";
                return View("Register", model);
            }

            if (!_otpCache.TryGetValue(model.Email, out var otpData))
            {
                ViewBag.Error = "Không tìm thấy OTP. Vui lòng yêu cầu lại.";
                return View("Register", new RegisterViewModel());
            }

            if (otpData.Expiry < DateTime.Now)
            {
                _otpCache.TryRemove(model.Email, out _);
                ViewBag.Error = "OTP đã hết hạn.";
                return View("Register", new RegisterViewModel());
            }

            if (otpData.Otp != model.Otp)
            {
                ViewBag.Error = "Mã OTP không hợp lệ.";
                model.OtpStatus = "OTP_SENT";
                return View("Register", model);
            }

            var newUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "Member"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            _otpCache.TryRemove(model.Email, out _);

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // ========================= LOGOUT =========================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ========================= PROFILE =========================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        public class UpdateProfileViewModel
        {
            [Required]
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty; // Email thường không cho phép sửa
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(UpdateProfileViewModel updatedModel)
        {
            if (!ModelState.IsValid) return View(updatedModel);

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            user.Username = updatedModel.Username;

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Profile");
        }

        // ========================= CHANGE PASSWORD =========================

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user?.PasswordHash))
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu cũ không đúng!");
                return View(model);
            }

            user!.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile");
        }

        // ========================= FORGOT PASSWORD =========================

        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        // Bước 1: Gửi OTP
        [HttpPost]
        [ActionName("ForgotPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendOtpForgotPassword(ForgotPasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || !new EmailAddressAttribute().IsValid(model.Email))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ.");
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Email này chưa được đăng ký.");
                return View(model);
            }

            // Xóa validation của các trường không liên quan
            foreach (var key in ModelState.Keys.Where(k => k != nameof(ForgotPasswordViewModel.Email)).ToList())
            {
                ModelState.Remove(key);
            }

            try
            {
                var otp = new Random().Next(100000, 999999).ToString();
                _otpCache[model.Email] = (otp, DateTime.Now.AddMinutes(5));

                await _emailService.SendEmailAsync(model.Email, "Mã OTP đặt lại mật khẩu", $"Mã OTP của bạn là: <b>{otp}</b>");

                model.OtpStatus = "OTP_SENT";
                ViewBag.Success = "Mã OTP đã được gửi tới email của bạn. Mã có hiệu lực trong 5 phút.";
                return View(model);
            }
            catch
            {
                ViewBag.Error = "Lỗi khi gửi email. Vui lòng thử lại.";
                return View(model);
            }
        }

        // Bước 2: Đặt lại mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ForgotPasswordViewModel model)
        {
            // Kiểm tra các trường cần thiết cho bước 2
            if (string.IsNullOrWhiteSpace(model.NewPassword) || string.IsNullOrWhiteSpace(model.Otp))
            {
                ViewBag.Error = "Vui lòng điền đầy đủ Mật khẩu mới và Mã OTP.";
                model.OtpStatus = "OTP_SENT";
                return View("ForgotPassword", model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu mới và xác nhận không khớp!";
                model.OtpStatus = "OTP_SENT";
                return View("ForgotPassword", model);
            }

            if (!_otpCache.TryGetValue(model.Email, out var otpData))
            {
                ViewBag.Error = "Không tìm thấy OTP. Vui lòng yêu cầu lại.";
                return View("ForgotPassword", new ForgotPasswordViewModel());
            }

            if (otpData.Expiry < DateTime.Now)
            {
                _otpCache.TryRemove(model.Email, out _);
                ViewBag.Error = "OTP đã hết hạn.";
                return View("ForgotPassword", new ForgotPasswordViewModel());
            }

            if (otpData.Otp != model.Otp)
            {
                ViewBag.Error = "Mã OTP không hợp lệ.";
                model.OtpStatus = "OTP_SENT";
                return View("ForgotPassword", model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
                return NotFound();

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            _otpCache.TryRemove(model.Email, out _);

            TempData["Success"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }
    }
}