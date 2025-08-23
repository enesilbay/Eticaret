using ETicaret.Core.Entities;
using ETicaret.Data;
using ETicaret.WebUI.Models;
using Microsoft.AspNetCore.Authentication;//login
using Microsoft.AspNetCore.Authorization;//login
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;//login
using System.Threading.Tasks;

namespace ETicaret.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataBaseContext _context;

        public AccountController(DataBaseContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
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
                    var account =await _context.AppUsers.FirstOrDefaultAsync(x=>x.Email == loginViewModel.Email
                    && x.Password==loginViewModel.Password && x.IsActive);
                    if (account == null)
                    {
                        ModelState.AddModelError("", "Giriş Başarısız");
                    }
                    else 
                    {
                        var claims=new List<Claim>()
                        {
                            new(ClaimTypes.Name,account.Name),
                            new(ClaimTypes.Role,account.IsAdmin ? "Admin" : "Customer"),
                            new(ClaimTypes.Email,account.Email),
                            new("UserId",account.Id.ToString()),
                            new("UserGuid",account.UserGuid.ToString())
                        };
                        var userIdentity = new ClaimsIdentity(claims,"Login");
                        ClaimsPrincipal userPrincipal = new ClaimsPrincipal(userIdentity);
                        await HttpContext.SignInAsync(userPrincipal);
                        return Redirect(string.IsNullOrEmpty(loginViewModel.ReturnUrl)? "/": loginViewModel.ReturnUrl);
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
            if (ModelState.IsValid)
            {
                await _context.AddAsync(appUser);
                await _context.SaveChangesAsync();
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
