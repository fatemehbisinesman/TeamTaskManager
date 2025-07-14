using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeamTaskManager.Models;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;
using TeamTaskManager.Data;

namespace TeamTaskManager.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            return View(await _context.Payments.ToListAsync());
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Payments/Create
        public IActionResult Create(int amount, string title, string userName)
        {
            var payment = new Payment
            {
                Amount = amount,
                PaymentFor = title,
                UserName = userName
            };
            return View(payment);
        }
     

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(payment);
                await _context.SaveChangesAsync();

                var client = new HttpClient();
                var request = new
                {
                    merchant_id = "00000000-0000-0000-0000-000000000000",
                    amount = payment.Amount,
                    description = $"پرداخت بابت {payment.PaymentFor}",
                    callback_url = Url.Action("Verify", "Payments", new { id = payment.Id }, Request.Scheme)
                };

                var response = await client.PostAsJsonAsync("https://sandbox.zarinpal.com/pg/v4/payment/request.json", request);
                var json = await response.Content.ReadAsStringAsync();

                ViewBag.RawResponse = json; // اینو آوردیم بالا تا همیشه در دسترس باشه

                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                if (result?.data?.code == 100)
                {
                    string authority = result.data.authority;
                    return Redirect($"https://sandbox.zarinpal.com/pg/StartPay/{authority}");
                }

                ViewBag.Error = "خطا در پرداخت: " + (result?.errors?.message ?? "پاسخی از سرور دریافت نشد.");
            }

            return View(payment);
        }

        public async Task<IActionResult> Verify(int id, string Authority, string Status)
        {
            if (Status != "OK")
            {
                ViewBag.Message = "پرداخت توسط کاربر لغو شد.";

                // 👇 حتی در این حالت هم می‌تونی وضعیت رو ذخیره کنی
                var canceledPayment = await _context.Payments.FindAsync(id);
                if (canceledPayment != null)
                {
                    canceledPayment.Status = "لغو شده توسط کاربر";
                    _context.Payments.Update(canceledPayment);
                    await _context.SaveChangesAsync();
                }

                return View();
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            var client = new HttpClient();
            var verifyRequest = new
            {
                merchant_id = "00000000-0000-0000-0000-000000000000", // کد تستی
                amount = payment.Amount,
                authority = Authority
            };

            var response = await client.PostAsJsonAsync("https://sandbox.zarinpal.com/pg/v4/payment/verify.json", verifyRequest);
            var json = await response.Content.ReadAsStringAsync();

            dynamic result = JsonConvert.DeserializeObject(json);

            if (result?.data?.code == 100)
            {
                ViewBag.Message = "پرداخت با موفقیت انجام شد.";
                ViewBag.RefId = result.data.ref_id;

                payment.Status = "پرداخت موفق"; // 👈 فارسی دقیق
                payment.PaymentDate = DateTime.Now;
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
            }
            else
            {
                ViewBag.Message = "پرداخت ناموفق بود: " + (result?.data?.message ?? "خطای نامشخص");
                payment.Status = "ناموفق"; // 👈 ذخیره وضعیت ناموفق
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
            }

            return View();
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            return View(payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserName,PaymentFor,Amount,PaymentDate,Status")] Payment payment)
        {
            if (id != payment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(payment);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}
