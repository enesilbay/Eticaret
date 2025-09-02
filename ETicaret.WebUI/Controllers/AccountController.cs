using ETicaret.Core.Entities;
using ETicaret.Service.Abstract;
using ETicaret.WebUI.Models;
using Microsoft.AspNetCore.Authentication;//login
using Microsoft.AspNetCore.Authorization;//login
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;//login

namespace ETicaret.WebUI.Controllers
{
    public class AccountController : Controller
    {
        
        //private readonly DataBaseContext _context;

        ////Dependency Injection
        //public AccountController(DataBaseContext context)
        //{
        //    _context = context;
        //}

        private readonly IService<AppUser> _service;

        public AccountController(IService<AppUser> service)
        {
            _service = service;
        }

        [Authorize]//Bu sayfayı sadece giriş yapmış kullanıcılar görebilir!" anlamına gelir.
        public async Task<IActionResult> Index()
        {
            //FirstOrDefault, sorguya uyan ilk öğeyi döndüren bir metodur.
            AppUser user = await _service.GetAsync(x=>x.UserGuid.ToString()==HttpContext.User.FindFirst("UserGuid").Value);
            if (user is null)
            {
                return NotFound();
            }
            var model = new UserEditViewModel()
            {
                Email = user.Email,
                Id = user.Id,
                Name = user.Name,
                Password = user.Password,
                Phone=user.Phone,
                Surname = user.Surname,
            };
            return View(model);
        }

        [HttpPost,Authorize]
        public async Task<IActionResult> IndexAsync(UserEditViewModel model)
        {
            if (ModelState.IsValid) 
            {
                try
                {
                    AppUser user = await _service.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
                    if (user is not null) 
                    {
                        user.Surname = model.Surname;
                        user.Phone = model.Phone;
                        user.Name = model.Name;
                        user.Password = model.Password;
                        user.Email = model.Email;
                        _service.Update(user);
                        var sonuc=_service.SaveChanges();

                        if (sonuc > 0)
                        {
                            TempData["Message"] = @"<div class=""alert alert-success alert-dismissible fade show"" role=""alert"">
                         <strong>Bilgileriniz Güncellenmiştir!</strong>
                            <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
                                </div>";
                            
                            return RedirectToAction("Index");
                        }
                    }
                    
                }
                catch (Exception) 
                {
                    ModelState.AddModelError("","Hata Oluştu");
                }
            }
            return View(model);
        }



        public IActionResult SignIn()//giriş
        {
            return View();
        }
       

        [HttpPost]
        public async Task<IActionResult> SignInAsync(LoginViewModel loginViewModel)//giriş
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    var account =await _service.GetAsync(x=>x.Email == loginViewModel.Email
                    && x.Password==loginViewModel.Password && x.IsActive);
                    if (account == null)
                    {
                        ModelState.AddModelError("", "Giriş Başarısız");
                    }
                    else 
                    {

                        //kimlik Doğrulama (Authentication)
                        var claims =new List<Claim>()
                        {
                            new(ClaimTypes.Name,account.Name),//Bu kişinin adı, veritabanından gelen account.Name değeridir."
                            new(ClaimTypes.Role,account.IsAdmin ? "Admin" : "Customer"),
                            new(ClaimTypes.Email,account.Email),
                            new("UserId",account.Id.ToString()),
                            new("UserGuid",account.UserGuid.ToString())
                        };
                        var userIdentity = new ClaimsIdentity(claims,"Login");
                        ClaimsPrincipal userPrincipal = new ClaimsPrincipal(userIdentity);
                        await HttpContext.SignInAsync(userPrincipal);
                        return Redirect(string.IsNullOrEmpty(loginViewModel.ReturnUrl)? "/": loginViewModel.ReturnUrl);
                        //Eğer kullanıcı korumalı bir sayfaya gitmeye çalışıp giriş sayfasına yönlendirildiyse,
                        //ReturnUrl dolu olur ve giriş yaptıktan sonra gitmek istediği o sayfaya geri döner.
                        //Eğer doğrudan giriş sayfasına geldiyse, anasayfaya (/) yönlendirilir.
                    }

                }
                catch (Exception hata)
                {
                    
                    ModelState.AddModelError("","Hata Oluştu!");
                }
            }
            return View(loginViewModel);
        }
        public IActionResult SignUp()//kayıt
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUpAsync(AppUser appUser)//kayıt
        {
            appUser.IsAdmin = false;
            appUser.IsActive = true;
            if (!ModelState.IsValid)
            {
                await _service.AddAsync(appUser);
                await _service.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(appUser);
          }

        public async Task<IActionResult> SignOutAsync()//çıkış
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("SignIn");
        }
    }
}
