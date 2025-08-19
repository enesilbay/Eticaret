using ETicaret.Core.Entities;
using ETicaret.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ETicaret.WebUI.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly DataBaseContext _context;

        public CategoriesController(DataBaseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> IndexAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.Include(p=>p.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
    }
}
