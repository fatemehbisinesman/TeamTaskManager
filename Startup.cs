using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeamTaskManager.Models;
using Microsoft.EntityFrameworkCore;
using TeamTaskManager.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TeamTaskManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // اضافه کردن سرویس‌ها
        public void ConfigureServices(IServiceCollection services)
        {
            // اتصال به دیتابیس
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // دسترسی به HttpContext (برای Session)
            services.AddHttpContextAccessor();

            // فعال کردن Session
            services.AddSession();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
         options.LoginPath = "/Auth/Login";  // مسیر صفحه لاگین
            options.AccessDeniedPath = "/Auth/AccessDenied"; // مسیر صفحه عدم دسترسی
            });

            services.AddAuthorization();

            services.AddControllersWithViews();


            // MVC
            services.AddControllersWithViews();
        }

        // پیکربندی pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();   

            app.UseAuthentication();


            app.UseAuthorization();

            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
