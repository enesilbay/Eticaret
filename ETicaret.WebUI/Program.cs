using ETicaret.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;



namespace ETicaret.WebUI
{

    public class Program
    {
        public static void Main(string[] args)
        {


            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<DataBaseContext>();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(x => 
                {
                    x.LoginPath = "/Account/SignIn";
                    x.AccessDeniedPath = "/AccessDenied";
                    x.Cookie.Name = "Account";
                    x.Cookie.MaxAge=TimeSpan.FromDays(7);
                    x.Cookie.IsEssential = true;
                }
                );

            builder.Services.AddAuthorization(x =>
            {
                x.AddPolicy("AdminPolicy",policy=>policy.RequireClaim(ClaimTypes.Role,"Admin"));
                x.AddPolicy("UserPolicy",policy=>policy.RequireClaim(ClaimTypes.Role,"Admin","User","Customer"));

            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();//önce oturum açma
            app.UseAuthorization();//sonra yetkilendirme

            app.MapStaticAssets();

            app.MapControllerRoute(
                        name: "admin",
                        pattern: "{area:exists}/{controller=Main}/{action=Index}/{id?}"
                      );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();


            app.Run();
        }
    }
}




