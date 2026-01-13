using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.Helpers;
using ActPro.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using System;

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
            builder.Configuration.GetConnectionString("DefaultConnection"),
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

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

            builder.Services.AddScoped<IAuditService, AuditService>();

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

            app.MapStaticAssets();

            app.MapRazorPages()
               .WithStaticAssets();

            app.MapControllerRoute("areaRoute", "{area:exists}/{controller}/{action}/{id?}");

            app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
