using Microsoft.AspNetCore.Mvc;

namespace ETicaret.WebUI.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult SignIn()//giriş
        {
            return View();
        }
        public IActionResult SignUp()//kayıt
        {
            return View();
        }
    }
}
