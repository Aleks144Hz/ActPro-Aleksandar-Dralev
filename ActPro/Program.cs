using ActPro.Helpers;
using ActPro.DAL;
using ActPro.DAL.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using System;

namespace ActPro
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(IdentityHelper.GetIdentityOptions)
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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
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
