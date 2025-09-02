using ETicaret.Core.Entities;
using ETicaret.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ETicaret.WebUI.Controllers
{
    [Authorize]
    public class MyAddressesController : Controller
    {
        private readonly IService<AppUser> _serviceAppUser;
        private readonly IService<Address> _serviceAddress;

        public MyAddressesController(IService<AppUser> service, IService<Address> serviceAddress)
        {
            _serviceAppUser = service;
            _serviceAddress = serviceAddress;
        }
        public  async Task<IActionResult> Index()
        {
            var appUser = await _serviceAppUser.GetAsync(x=>x.UserGuid.ToString()==HttpContext.User.FindFirst("UserGuid").Value);

            if (appUser == null)
            {
                return NotFound("Kullanıcı Datası Bulunamadı! Oturumunuzu Kapatıp Lütfen Tekrar Giriş Yapınız");
            }
            var model = await _serviceAddress.GetAllAsync(u=>u.AppUserId == appUser.Id);
            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Address address)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
                    if (appUser != null)
                    {
                        address.AppUserId = appUser.Id;
                        _serviceAddress.Add(address);
                        await _serviceAddress.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception)
                {
                   ModelState.AddModelError("", "Adres Eklenirken Bir Hata Oluştu! Lütfen Tüm Alanları Kontrol Edip Tekrar Deneyiniz");
                }
            }
            ModelState.AddModelError("", "Kayıt Başarısız");
            return View(address);
        }



    }
}
