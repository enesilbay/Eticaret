using ETicaret.Core.Entities;
using ETicaret.Service.Abstract;
using ETicaret.WebUI.ExtensionMethods;
using Microsoft.AspNetCore.Mvc;

namespace ETicaret.WebUI.Controllers
{
    public class FavoritesController : Controller
    {

        private readonly IService<Product> _service;

        public FavoritesController(IService<Product> service)
        {
            _service = service;
        }


        public IActionResult Index()
        {
            var favoriler = GetFavorites();  //GetFavorites metodunu çağırarak gelen listeyi favoriler e atar ve onu da view'a gönderir.
            return View(favoriler);
        }

        private List<Product> GetFavorites()
        {
            return HttpContext.Session.GetJson<List<Product>>("GetFavorites") ?? [];
        }

        public IActionResult Add(int ProductId)
        {
            var favoriler = GetFavorites();
            var product = _service.Find(ProductId);

            if (product != null && !favoriler.Any(p => p.Id == ProductId))
            {
                favoriler.Add(product);
                HttpContext.Session.SetJson("GetFavorites", favoriler);

                TempData["Message"] = @"<div class=""alert alert-success alert-dismissible fade show"" role=""alert"">
                                  <strong>Ürün Favorilerinize Eklenmiştir!</strong>
                                  <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
                               </div>";
            }

            // Her durumda ürünün bulunduğu sayfaya geri dön
            return Redirect(Request.Headers["referer"].ToString());
        }


        public IActionResult Remove(int ProductId)
        {
            var favoriler = GetFavorites();
            var product = _service.Find(ProductId);
            if (product != null && favoriler.Any(p => p.Id == ProductId)) 
            {
                favoriler.RemoveAll(i=>i.Id == product.Id);
                HttpContext.Session.SetJson("GetFavorites", favoriler);
            }
            return RedirectToAction("Index");
        }


    }
}
