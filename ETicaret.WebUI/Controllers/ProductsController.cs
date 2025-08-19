using ETicaret.Data;
using ETicaret.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.WebUI.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DataBaseContext _context;

        public ProductsController(DataBaseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string q="")
        {
            var dataBaseContext = _context.Products.Where(p=>p.IsActive && p.Name.Contains(q) || p.Description.Contains(q)   ).Include(p => p.Brand).Include(p => p.Category);
            return View(await dataBaseContext.ToListAsync());
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
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
