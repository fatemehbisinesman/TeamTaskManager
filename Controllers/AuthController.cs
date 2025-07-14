using Microsoft.AspNetCore.Mvc;
using TeamTaskManager.Models;
using TeamTaskManager.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Identity;
using TeamTaskManager.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;

namespace TeamTaskManager.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET: ثبت‌نام
        public IActionResult Register() => View();

        // ✅ POST: ثبت‌نام با reCAPTCHA
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            // بررسی تکراری بودن ایمیل
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                TempData["ErrorMessage"] = "ایمیل وارد شده قبلاً ثبت شده است.";
                return RedirectToAction("Register");
            }

            // بررسی reCAPTCHA
            var recaptcha = Request.Form["g-recaptcha-response"];
            var secret = "6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe"; // کلید تست گوگل

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={recaptcha}",
                    null);

                var result = await response.Content.ReadAsStringAsync();
                dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(result);

                if (json.success != true)
                {
                    TempData["ErrorMessage"] = "لطفاً تأیید کنید که ربات نیستید.";
                    return RedirectToAction("Register");
                }
            }

            if (ModelState.IsValid)
            {
                // بررسی اینکه اگر کاربر بخواد Admin باشه و قبلاً Admin وجود داره، ردش کنیم
                if (user.Role == "Admin" && _context.Users.Any(u => u.Role == "Admin"))
                {
                    TempData["ErrorMessage"] = "فقط یک مدیر مجاز است. لطفاً به‌عنوان کاربر عادی ثبت‌نام کنید.";
                    return RedirectToAction("Register");

                }

                var hasher = new PasswordHasher<User>();
                user.Password = hasher.HashPassword(user, user.Password);

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "ثبت‌نام با موفقیت انجام شد.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "لطفاً تمام فیلدها را به‌درستی تکمیل کنید.");

            return View(user);
        }
        [HttpGet]
        public IActionResult Login() => View();


        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // ✅ بررسی reCAPTCHA
            var recaptcha = Request.Form["g-recaptcha-response"];
            var secret = "6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe"; // کلید تست گوگل

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={recaptcha}",
                    null);

                var result = await response.Content.ReadAsStringAsync();
                dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(result);

                if (json.success != true)
                {
                    ViewBag.Error = "لطفاً تأیید کنید که ربات نیستید.";
                    return View();  // ورود متوقف میشه
                }
            }

            // ادامه ورود
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                var hasher = new PasswordHasher<User>();
                var result = hasher.VerifyHashedPassword(user, user.Password, password);

                if (result == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserRole", user.Role);

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return user.Role == "Admin"
                        ? RedirectToAction("Dashboard", "Admin")
                        : RedirectToAction("Dashboard", "Users");
                }

                ViewBag.Error = "رمز عبور اشتباه است.";
            }
            else
            {
                ViewBag.Error = "ایمیل یافت نشد.";
            }

            return View();
        }
        // ✅ خروج
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ✅ GET: فراموشی رمز عبور
        public IActionResult ForgotPassword() => View();

        // ✅ POST: ارسال ایمیل برای بازیابی
        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["Message"] = "کاربری با این ایمیل یافت نشد.";
                return RedirectToAction("ForgotPassword");
            }

            return RedirectToAction("ResetPassword", new { email = user.Email });
        }

        // ✅ GET: فرم وارد کردن رمز جدید
        public IActionResult ResetPassword(string email)
        {
            var model = new ForgotPasswordViewModel { Email = email };
            return View(model);
        }

        // ✅ POST: ذخیره رمز جدید
        [HttpPost]
        public IActionResult ResetPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "کاربر یافت نشد.");
                return View(model);
            }

            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, model.NewPassword);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "رمز عبور با موفقیت تغییر کرد.";
            return RedirectToAction("Login");
        }

    }
}