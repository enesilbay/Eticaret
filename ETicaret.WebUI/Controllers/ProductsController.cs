using ETicaret.Core.Entities;
using ETicaret.Data;
using ETicaret.Service.Abstract;
using ETicaret.Service.Concrete;
using ETicaret.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.WebUI.Controllers
{
    public class ProductsController : Controller
    {
        // //_context değişkenini kendimiz oluşturmuyoruz, DI konteyneri bize veriyor.
        // private readonly DataBaseContext _context;

        // // Controller oluşturulurken, servis olarak kaydettiğimiz
        // // DataBaseContext otomatik olarak buraya "enjekte edilir".
        //public ProductsController(DataBaseContext context)
        // {
        //     _context = context;
        // }

        private readonly IService<Product> _service;

        public ProductsController(IService<Product> service)
        {
            _service = service;
        }


        public async Task<IActionResult> Index(string q="")
        {//.Include(p => p.Brand).Include(p => p.Category)
            var dataBaseContext = _service.GetAllAsync(p=>p.IsActive && p.Name.Contains(q) || p.Description.Contains(q));
            return View(await dataBaseContext);
// await kelimesi, bu işlem bitene kadar sunucunun başka işler yapabilmesini sağlar (asenkron çalışma).
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _service.GetQueryable()
                //Eager Loading
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            var model = new ProductDetailViewModel()
            {
                Product = product,
                RelatedProducts = _service.GetQueryable().Where(p => p.IsActive && p.CategoryId== product.CategoryId && p.Id != product.Id)
            };
            return View(model);
        }
    }
}
