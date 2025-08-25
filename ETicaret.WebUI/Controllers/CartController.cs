using ETicaret.Core.Entities;
using ETicaret.Service.Abstract;
using ETicaret.Service.Concrete;
using ETicaret.WebUI.ExtensionMethods;
using ETicaret.WebUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ETicaret.WebUI.Controllers
{
    public class CartController : Controller
    {
        private readonly IService<Product> _service;

        public CartController(IService<Product> service)
        {
            _service = service;
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

        public IActionResult Add(int ProductId,int quantity=1)
        {
            var product = _service.Find(ProductId);

            if (product != null)
            {
                var cart = GetCart();
                cart.AddProduct(product,quantity);
                HttpContext.Session.SetJson("Cart",cart);
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
                HttpContext.Session.SetJson("Cart",cart);
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




        private CartService GetCart()
        {
            return HttpContext.Session.GetJson<CartService>("Cart") ?? new CartService();
        }
        


    }
}
