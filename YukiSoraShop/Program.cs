
using Infrastructure;
using YukiSoraShop.Filters;

namespace YukiSoraShop
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container (add global page exception filter)
            builder.Services
                .AddRazorPages()
                .AddMvcOptions(o => { o.Filters.Add<GlobalExceptionPageFilter>(); });

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
            // Razor Pages-style error handling: use /Error for exceptions and status codes
            app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
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
