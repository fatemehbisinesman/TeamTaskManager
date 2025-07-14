using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TeamTaskManager.Helpers
{
    public class ZarinpalClassic
    {
        private readonly string merchantId = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";// 🔁 مرچنت تستی رو اینجا جایگزین کن

        private const string requestUrl = "https://sandbox.zarinpal.com/pg/v4/payment/request.json";
        private const string verifyUrl = "https://sandbox.zarinpal.com/pg/v4/payment/verify.json";

        public async Task<(bool Success, string Authority, string ErrorMessage)> RequestAsync(int amount, string description, string callbackUrl)
        {
            var client = new HttpClient();

            var data = new
            {
                merchant_id = merchantId,
                amount = amount,
                description = description,
                callback_url = callbackUrl
            };

            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(requestUrl, content);
            var resultString = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(resultString);
                var root = doc.RootElement;

                var status = root.GetProperty("data").GetProperty("code").GetInt32();
                if (status == 100)
                {
                    var authority = root.GetProperty("data").GetProperty("authority").GetString();
                    return (true, authority, null);
                }

                var error = root.GetProperty("errors").GetProperty("message").GetString();
                return (false, null, "کد خطا: " + status + " - " + error);
            }
            catch (Exception)
            {
                return (false, null, "پاسخ نامعتبر از زرین‌پال:\n" + resultString);
            }
        }

        public async Task<(bool Success, string RefId, string ErrorMessage)> VerifyAsync(int amount, string authority)
        {
            var client = new HttpClient();

            var data = new
            {
                merchant_id = merchantId,
                amount = amount,
                authority = authority
            };

            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(verifyUrl, content);
            var resultString = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(resultString);
                var root = doc.RootElement;

                var status = root.GetProperty("data").GetProperty("code").GetInt32();
                if (status == 100)
                {
                    var refId = root.GetProperty("data").GetProperty("ref_id").GetRawText();
                    return (true, refId, null);
                }

                var error = root.GetProperty("errors").GetProperty("message").GetString();
                return (false, null, "کد خطا: " + status + " - " + error);
            }
            catch (Exception)
            {
                return (false, null, "پاسخ نامعتبر از زرین‌پال:\n" + resultString);
            }
        }
    }
}