using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CMCS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllersWithViews();

            // Enable session support
            builder.Services.AddDistributedMemoryCache();  // Adds a default in-memory implementation of IDistributedCache
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Sets the session timeout duration
                options.Cookie.HttpOnly = true; // Makes the session cookie accessible only to HTTP requests
                options.Cookie.IsEssential = true; // Marks the session cookie as essential for GDPR compliance
            });

            // Add services for Azure Storage (TableService, FileService)
            builder.Services.AddSingleton<TableService>();
            builder.Services.AddSingleton<FileService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Enable session and authentication/authorization middleware
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
