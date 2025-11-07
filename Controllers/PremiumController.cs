using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopify.Data;
using Shopify.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.Controllers
{
    [Authorize]
    public class PremiumController : Controller
    {
        private readonly MusicDbContext _context;
        private readonly IConfiguration _config;

        public PremiumController(MusicDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // -----------------------------
        // Hiển thị danh sách gói Premium
        // -----------------------------
        public async Task<IActionResult> Index()
        {
            var premiums = await _context.Premiums
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .ToListAsync();
            return View(premiums);
        }

        // -----------------------------
        // Tạo link thanh toán VNPay
        // -----------------------------
        public IActionResult Pay(int id)
        {
            var premium = _context.Premiums.Find(id);
            if (premium == null) return NotFound();

            // 🔹 Lấy User đang đăng nhập (giả sử lưu ID trong Claims)
            int userId = int.Parse(User.FindFirst("UserId").Value);

            // 🔹 Tạo bản ghi Payment (trạng thái Pending)
            var payment = new Payment
            {
                UserId = userId,
                PremiumId = premium.PremiumId,
                Amount = premium.Price,
                Method = "VNPay",
                Status = "Pending"
            };
            _context.Payments.Add(payment);
            _context.SaveChanges();

            // 🔹 Lấy cấu hình VNPay
            string baseUrl = _config["VNPay:BaseUrl"];
            string returnUrl = _config["VNPay:ReturnUrl"];
            string tmnCode = _config["VNPay:TmnCode"];
            string hashSecret = _config["VNPay:HashSecret"];
            string version = _config["VNPay:Version"];
            string command = _config["VNPay:Command"];
            string currCode = _config["VNPay:CurrCode"];
            string locale = _config["VNPay:Locale"];

            // 🔹 Thông tin đơn hàng
            string txnRef = payment.PaymentId.ToString(); // Dùng ID Payment để map khi callback
            string amount = ((int)premium.Price * 100).ToString();
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            // 🔹 Tạo các tham số VNPay
            var vnp_Params = new SortedDictionary<string, string>
            {
                {"vnp_Version", version},
                {"vnp_Command", command},
                {"vnp_TmnCode", tmnCode},
                {"vnp_Amount", amount},
                {"vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")},
                {"vnp_CurrCode", currCode},
                {"vnp_IpAddr", ipAddress},
                {"vnp_Locale", locale},
                {"vnp_OrderInfo", $"Thanh toán gói Premium: {premium.Name}"},
                {"vnp_OrderType", "other"},
                {"vnp_ReturnUrl", returnUrl},
                {"vnp_TxnRef", txnRef}
            };

            // 🔹 Tạo chuỗi ký
            var rawData = string.Join("&", vnp_Params.Select(kvp =>
                $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
            var signData = HmacSHA512(hashSecret, rawData);

            // 🔹 URL thanh toán
            var paymentUrl = $"{baseUrl}?{rawData}&vnp_SecureHash={signData}";

            return Redirect(paymentUrl);
        }

        // -----------------------------
        // Nhận phản hồi từ VNPay
        // -----------------------------
        [HttpGet("/api/payment/vnpay-return")]
        public IActionResult VNPayReturn()
        {
            var query = HttpContext.Request.Query;

            // Lấy tất cả tham số VNPay (trừ hash)
            var vnpData = query
                .Where(kvp => kvp.Key.StartsWith("vnp_") &&
                              kvp.Key != "vnp_SecureHash" &&
                              kvp.Key != "vnp_SecureHashType")
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());

            var sorted = new SortedDictionary<string, string>(vnpData);
            string rawData = string.Join("&", sorted.Select(kvp =>
                $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

            string secureHash = query["vnp_SecureHash"];
            string hashSecret = _config["VNPay:HashSecret"];
            string computedHash = HmacSHA512(hashSecret, rawData);

            bool validSignature = secureHash.Equals(computedHash, StringComparison.InvariantCultureIgnoreCase);
            string responseCode = query["vnp_ResponseCode"];
            string txnRef = query["vnp_TxnRef"];
            string transactionNo = query["vnp_TransactionNo"];

            // Tìm lại Payment
            var payment = _context.Payments.FirstOrDefault(p => p.PaymentId.ToString() == txnRef);

            if (payment == null)
            {
                ViewBag.Message = "❌ Không tìm thấy giao dịch.";
                return View("PaymentResult");
            }

            if (!validSignature)
            {
                payment.Status = "Failed";
                _context.SaveChanges();
                ViewBag.Message = "❌ Sai chữ ký VNPay (Invalid signature)";
                return View("PaymentResult");
            }

            // ✅ Nếu thanh toán thành công
            if (responseCode == "00")
            {
                payment.Status = "Success";
                payment.TransactionId = transactionNo;
                payment.ResponseCode = responseCode;
                _context.SaveChanges();

                ViewBag.Message = "✅ Thanh toán thành công!";
                ViewBag.OrderInfo = query["vnp_OrderInfo"];
            }
            else
            {
                payment.Status = "Failed";
                payment.ResponseCode = responseCode;
                _context.SaveChanges();

                ViewBag.Message = $"❌ Thanh toán thất bại. Mã lỗi: {responseCode}";
            }

            return View("PaymentResult");
        }

        // -----------------------------
        // HMAC SHA512 helper
        // -----------------------------
        private static string HmacSHA512(string key, string input)
        {
            var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
