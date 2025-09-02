using ETicaret.Core.Entities;
using ETicaret.Service.Abstract;
using ETicaret.Service.Concrete;
using ETicaret.WebUI.ExtensionMethods;
using ETicaret.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ETicaret.WebUI.Controllers
{
    public class CartController : Controller
    {
        private readonly IService<Product> _service;
        private readonly IService<Address> _serviceAddress;
        private readonly IService<AppUser> _serviceAppUser;

        public CartController(IService<Product> service, IService<Address> serviceAddress, IService<AppUser> serviceAppUser)
        {
            _service = service;
            _serviceAddress = serviceAddress;
            _serviceAppUser = serviceAppUser;
        }
        public IActionResult Index()
        {
            var cart=GetCart();
            var model = new CartViewModel()
            {
                CartLines=cart.CartLines,   
                TotalPrice=cart.TotalPrice()
            };
            return View(model);
        }

        public IActionResult Add(int ProductId, int quantity = 1)
        {
            var product = _service.Find(ProductId);

            if (product != null)
            {
                var cart = GetCart();
                cart.AddProduct(product, quantity);
                HttpContext.Session.SetJson("Cart", cart);
                TempData["Message"] = @"<div class=""alert alert-success alert-dismissible fade show"" role=""alert"">
                                  <strong>Ürün sepetinize eklenmiştir!</strong>
                                  <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
                               </div>";    
                return Redirect(Request.Headers["referer"].ToString());
            }
            return RedirectToAction("Index");
        }




        public IActionResult Update(int ProductId,int quantity=1)
        {
            var product = _service.Find(ProductId);

            if (product != null)
            {
                var cart = GetCart();
                cart.UpdateProduct(product,quantity);
                HttpContext.Session.SetJson("Cart",cart); //güncellenen sepet verisini JSON formatında session'a kaydeder. Bu, oturum sona erene kadar kullanıcının sepetinin kaybolmamasını sağlar.
            }


            return RedirectToAction("Index");
        }


        public IActionResult Remove(int ProductId)
        {
            var product = _service.Find(ProductId);

            if (product != null)
            {
                var cart = GetCart();
                cart.RemoveProduct(product);
                HttpContext.Session.SetJson("Cart",cart);
            }


            return RedirectToAction("Index");
        }
        [Authorize]
        public async Task<IActionResult> CheckOut() 
        {
            var cart = GetCart();
            var appUser = await _serviceAppUser.GetAsync(x=> x.UserGuid.ToString()==HttpContext.User.FindFirst("UserGuid").Value);
            if (appUser == null)
            {
                return RedirectToAction("SıgnIn", "Account");
            }
            var addresses = await _serviceAddress.GetAllAsync(a => a.AppUserId == appUser.Id && a.IsActive);
            var model = new CheckOutViewModel()
            {
                CartProducts = cart.CartLines,
                TotalPrice = cart.TotalPrice(),
                Addresses=addresses
            };
            return View(model);
        }



        private CartService GetCart()
        {
            return HttpContext.Session.GetJson<CartService>("Cart") ?? new CartService();
        }
        


    }
}
