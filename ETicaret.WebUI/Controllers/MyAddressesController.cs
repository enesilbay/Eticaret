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

        public async Task<IActionResult> Edit(string id)
        {
            var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
            if (appUser == null)
            {
                return NotFound("Kullanıcı Datası Bulunamadı! Oturumunuzu Kapatıp Lütfen Tekrar Giriş Yapınız");
            }
            var model = await _serviceAddress.GetAsync(u => u.AdressGuid.ToString() ==id && u.AppUserId==appUser.Id);
            if(model == null)
            {
                return NotFound("Adres Bilgisi Bulunamadı!");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id,Address address)
        {
            var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
            if (appUser == null)
            {
                return NotFound("Kullanıcı Datası Bulunamadı! Oturumunuzu Kapatıp Lütfen Tekrar Giriş Yapınız");
            }
            var model = await _serviceAddress.GetAsync(u => u.AdressGuid.ToString() ==id && u.AppUserId==appUser.Id);
            if(model == null) 
                return NotFound("Adres Bilgisi Bulunamadı!");      
                model.Title = address.Title;
                model.City = address.City;
                model.District = address.District;
                model.OpenAddress = address.OpenAddress;
                model.IsBillingAddress = address.IsBillingAddress;
                model.IsDeliveryAddress = address.IsDeliveryAddress;
                model.IsActive = address.IsActive;
            //Kullanıcının düzenlediği adres dışındaki bütün diğer adreslerini veritabanından çeker.
            var otherAddresses = await _serviceAddress.GetAllAsync(x=>x.AppUserId==appUser.Id && x.Id != model.Id);

            //Varsayılan Adres Mantığı
            foreach (var otherAddress in otherAddresses)
                {
                   otherAddress.IsBillingAddress = false;
                    otherAddress.IsDeliveryAddress = false;
                     _serviceAddress.Update(otherAddress);
                }

                try
                    {
                        _serviceAddress.Update(model);
                        await _serviceAddress.SaveChangesAsync();
                        return RedirectToAction("Index");
                       
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", "hata oluştu");
                    }
            return View(model);
        }


        public async Task<IActionResult> Delete(string id)
        {
            var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
            if (appUser == null)
            {
                return NotFound("Kullanıcı Datası Bulunamadı! Oturumunuzu Kapatıp Lütfen Tekrar Giriş Yapınız");
            }
            var model = await _serviceAddress.GetAsync(u => u.AdressGuid.ToString() == id && u.AppUserId == appUser.Id);
            if (model == null)
            {
                return NotFound("Adres Bilgisi Bulunamadı!");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id,Address address)
        {
            var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
            if (appUser == null)
            {
                return NotFound("Kullanıcı Datası Bulunamadı! Oturumunuzu Kapatıp Lütfen Tekrar Giriş Yapınız");
            }
            var model = await _serviceAddress.GetAsync(u => u.AdressGuid.ToString() == id && u.AppUserId == appUser.Id);
            if (model == null)
                return NotFound("Adres Bilgisi Bulunamadı!");
            try
            {
                _serviceAddress.Delete(model);
                await _serviceAddress.SaveChangesAsync();
                return RedirectToAction("Index");

            }
            catch (Exception) 
            {
                ModelState.AddModelError("", "hata oluştu");
            }
            return View(model);
        }




    }
}
