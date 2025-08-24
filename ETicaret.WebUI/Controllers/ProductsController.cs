using ETicaret.Data;
using ETicaret.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.WebUI.Controllers
{
    public class ProductsController : Controller
    {
        //_context değişkenini kendimiz oluşturmuyoruz, DI konteyneri bize veriyor.
        private readonly DataBaseContext _context;

        // Controller oluşturulurken, servis olarak kaydettiğimiz
        // DataBaseContext otomatik olarak buraya "enjekte edilir".
       public ProductsController(DataBaseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string q="")
        {
            var dataBaseContext = _context.Products.Where(p=>p.IsActive && p.Name.Contains(q) || p.Description.Contains(q)   ).Include(p => p.Brand).Include(p => p.Category);
            return View(await dataBaseContext.ToListAsync());
// await kelimesi, bu işlem bitene kadar sunucunun başka işler yapabilmesini sağlar (asenkron çalışma).
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
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
                RelatedProducts = _context.Products.Where(p => p.IsActive && p.CategoryId== product.CategoryId && p.Id != product.Id)
            };
            return View(model);
        }
    }
}
