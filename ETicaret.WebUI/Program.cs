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


            var builder = WebApplication.CreateBuilder(args);//web uygulamasýný oluþturmak ve yapýlandýrmak için

            // Add services to the container.
            builder.Services.AddControllersWithViews();//Controller ve View'larýn çalýþmasý
                                                       //için gereken tüm temel servisleri kaydeder.

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
            //Bu kod satýrý, IService<T> arayüzünü Service<T> sýnýfýna baðlar.
            //Yani, uygulama her IService<T> talep ettiðinde, ona uygun Service<T> örneði verilecektir.



            //uygulamanýn kimlik doðrulama sistemini kurar.
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(x => 
                {
                    x.LoginPath = "/Account/SignIn";//Eðer kullanýcý giriþ yapmamýþsa ama giriþ gerektiren bir sayfaya gitmeye çalýþýrsa, otomatik olarak bu adrese yönlendirilir.
                    x.AccessDeniedPath = "/AccessDenied";//ullanýcý giriþ yapmýþ ancak eriþmeye çalýþtýðý sayfa için yeterli yetkisi yoksa
                    x.Cookie.Name = "Account";
                    x.Cookie.MaxAge=TimeSpan.FromDays(7);//Çerezin geçerlilik süresini 7 gün olarak ayarlar.
                    x.Cookie.IsEssential = true;
                }
                );


            // yetkilendirme kurallarýný, yani "Policy" (Politika)'leri tanýmlar.
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


            app.UseRouting();//Gelen isteðin URL'sine bakarak hangi Controller ve Action'ýn
           //çalýþmasý gerektiðine karar veren yönlendirme (routing) mekanizmasýný devreye sokar.

            app.UseSession();
                             

            app.UseAuthentication();//önce oturum açma
            //Bu ara yazýlým, gelen istekle birlikte gelen çerezi kontrol eder
            // Eðer geçerli bir çerez varsa, kullanýcýnýn kim olduðunu belirler
            //ve kimlik bilgilerini sisteme tanýtýr. UseAuthorization'dan önce gelmelidir. 


            app.UseAuthorization();//sonra yetkilendirme,Kimliði belirlenmiþ
             //olan kullanýcýnýn, gitmek istediði sayfaya eriþim yetkisi olup olmadýðýný
             //kontrol eder. Örneðin, [Authorize(Policy="AdminPolicy")]
             //ile iþaretlenmiþ bir sayfaya sadece "Admin" rolündeki
             //kullanýcýlarýn girmesine izin verir.


            app.MapStaticAssets();//URL yönlendirme kurallarýný tanýmlar.
            


            app.MapControllerRoute(//Yönlendirme kurallarý isteði fiziksel olarak ilgili
 //Controller metoduna iletir.ProductsController'daki Details(5) metodu çalýþtýrýlýr.
//Metodun içindeki kod çalýþýr, veritabanýndan 5 numaralý ürün bulunur
//ve HTML sayfasý oluþturularak USerýn tarayýcýsýna geri gönderilir
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




