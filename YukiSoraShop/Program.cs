using Application;
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
                    options.Cookie.Name = ".YukiSora.Auth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax; // protect CSRF while allowing normal nav
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // send over HTTPS only

                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/Login";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.SlidingExpiration = true;
                });

            builder.Services.AddAuthorization();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>(); // <-- THÊM DÒNG NÀY

            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddPaymentServices(builder.Configuration);



            // Add session services
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.Name = ".YukiSora.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddPaymentServices(builder.Configuration);

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
            // Redirect root to Home landing page
            app.MapGet("/", () => Results.Redirect("/Home"));

            app.Run();
        }
    }
}
