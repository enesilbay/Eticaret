using ETicaret.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using ETicaret.Service.Abstract;
using ETicaret.Service.Concrete;



namespace ETicaret.WebUI
{

    public class Program
    {
        public static void Main(string[] args)
        {


            var builder = WebApplication.CreateBuilder(args);//web uygulamas�n� olu�turmak ve yap�land�rmak i�in

            // Add services to the container.
            builder.Services.AddControllersWithViews();//Controller ve View'lar�n �al��mas�
                                                       //i�in gereken t�m temel servisleri kaydeder.

            builder.Services.AddSession(Options =>
            {
                Options.Cookie.Name = ".ETicaret.Session";
                Options.Cookie.HttpOnly = true;
                Options.Cookie.IsEssential = true;
                Options.IdleTimeout = TimeSpan.FromDays(1);
                Options.IOTimeout = TimeSpan.FromMinutes(10);
            });


            //"Dependency Injection Design Pattern
            builder.Services.AddDbContext<DataBaseContext>();

            builder.Services.AddScoped(typeof(IService<>),typeof(Service<>));
            //Bu kod sat�r�, IService<T> aray�z�n� Service<T> s�n�f�na ba�lar.
            //Yani, uygulama her IService<T> talep etti�inde, ona uygun Service<T> �rne�i verilecektir.



            //uygulaman�n kimlik do�rulama sistemini kurar.
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(x => 
                {
                    x.LoginPath = "/Account/SignIn";//E�er kullan�c� giri� yapmam��sa ama giri� gerektiren bir sayfaya gitmeye �al���rsa, otomatik olarak bu adrese y�nlendirilir.
                    x.AccessDeniedPath = "/AccessDenied";//ullan�c� giri� yapm�� ancak eri�meye �al��t��� sayfa i�in yeterli yetkisi yoksa
                    x.Cookie.Name = "Account";
                    x.Cookie.MaxAge=TimeSpan.FromDays(7);//�erezin ge�erlilik s�resini 7 g�n olarak ayarlar.
                    x.Cookie.IsEssential = true;
                }
                );


            // yetkilendirme kurallar�n�, yani "Policy" (Politika)'leri tan�mlar.
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


            app.UseRouting();//Gelen iste�in URL'sine bakarak hangi Controller ve Action'�n
           //�al��mas� gerekti�ine karar veren y�nlendirme (routing) mekanizmas�n� devreye sokar.

            app.UseSession();
                             

            app.UseAuthentication();//�nce oturum a�ma
            //Bu ara yaz�l�m, gelen istekle birlikte gelen �erezi kontrol eder
            // E�er ge�erli bir �erez varsa, kullan�c�n�n kim oldu�unu belirler
            //ve kimlik bilgilerini sisteme tan�t�r. UseAuthorization'dan �nce gelmelidir. 


            app.UseAuthorization();//sonra yetkilendirme,Kimli�i belirlenmi�
             //olan kullan�c�n�n, gitmek istedi�i sayfaya eri�im yetkisi olup olmad���n�
             //kontrol eder. �rne�in, [Authorize(Policy="AdminPolicy")]
             //ile i�aretlenmi� bir sayfaya sadece "Admin" rol�ndeki
             //kullan�c�lar�n girmesine izin verir.


            app.MapStaticAssets();//URL y�nlendirme kurallar�n� tan�mlar.
            


            app.MapControllerRoute(//Y�nlendirme kurallar� iste�i fiziksel olarak ilgili
 //Controller metoduna iletir.ProductsController'daki Details(5) metodu �al��t�r�l�r.
//Metodun i�indeki kod �al���r, veritaban�ndan 5 numaral� �r�n bulunur
//ve HTML sayfas� olu�turularak USer�n taray�c�s�na geri g�nderilir
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




