using Application.Services;
using Application.Services.Interfaces;
using Infrastructure;

namespace YukiSoraShop
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container
            builder.Services.AddRazorPages();

            // Add Authentication services
            builder.Services.AddAuthentication("CookieAuth")
                .AddCookie("CookieAuth", options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/Login";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.SlidingExpiration = true;
                });

            builder.Services.AddAuthorization();

            // Add session services
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddPaymentServices(builder.Configuration);

            // Register custom services
            builder.Services.AddScoped<YukiSoraShop.Services.Interfaces.IAuthorizationService, YukiSoraShop.Services.AuthorizationService>();
            // HttpContextAccessor is already registered in payment DI; remove duplicate

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();


            app.Run();
        }
    }
}
