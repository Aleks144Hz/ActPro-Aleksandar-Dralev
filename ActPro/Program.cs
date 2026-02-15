using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.Domain.Repository;
using ActPro.Helpers;
using ActPro.Services;
using ActPro.Services.Interfaces;
using ActPro.Services.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActPro
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var cultureInfo = new System.Globalization.CultureInfo("bg-BG");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (Environment.GetEnvironmentVariable("RENDER") == "true")
            {
                var cloudString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
                if (!string.IsNullOrEmpty(cloudString))
                {
                    connectionString = cloudString;
                }
            }
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
            connectionString,
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
            .UseCompatibilityLevel(110)));

            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(IdentityHelper.GetIdentityOptions)
                            .AddErrorDescriber<BulgarianIdentityErrorDescriber>()
                            .AddEntityFrameworkStores<ApplicationDbContext>()
                            .AddDefaultTokenProviders();
            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })
            .AddDataAnnotationsLocalization()
            .AddViewLocalization();


            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")));

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

            builder.Services.AddScoped<IHomeService, HomeService>();

            builder.Services.AddScoped<IAccountService, AccountService>();

            builder.Services.AddScoped<ISearchService, SearchService>();

            builder.Services.AddScoped<IReservationService, ReservationService>();

            builder.Services.AddScoped<IPlaceService, PlaceService>();

            builder.Services.AddScoped<IOwnerDashboardService, OwnerDashboardService>();

            builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

            builder.Services.AddScoped<IReservationDashboardService, IReservationDashboardService>();

            builder.Services.AddScoped<IPlaceDashboardService, PlaceDashboardService>();

            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<IAuditService, AuditService>();

            builder.Services.AddScoped<IAuditDashboardService, AuditDashboardService>();

            builder.Services.AddTransient<ActPro.Services.Interfaces.IEmailSender, EmailSender>();

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");

                app.UseHsts();
            }
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    await DbSeeder.SeedRolesAndAdminAsync(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Ãðåøêà ïðè ñúçäàâàíåòî íà Àäìèíà.");
                }
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapRazorPages();

            app.MapControllerRoute("areaRoute", "{area:exists}/{controller}/{action}/{id?}");

            app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
