using ETicaret.Core.Entities;
using ETicaret.WebUI.ExtensionMethods;
using Microsoft.AspNetCore.Mvc;

namespace ETicaret.WebUI.Controllers
{
    public class FavoritesController : Controller
    {
        public IActionResult Index()
        {
            var favoriler = GetFavorites();  //GetFavorites metodunu çağırarak gelen listeyi favoriler e atar ve onu da view'a gönderir.
            return View(favoriler);
        }

        private List<Product> GetFavorites()
        {
            return HttpContext.Session.GetJson<List<Product>>("GetFavorites") ?? [];
        }


    }
}
