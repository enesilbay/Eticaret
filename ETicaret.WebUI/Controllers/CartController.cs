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
        private readonly IService<Order> _serviceOrder;

        public CartController(IService<Product> service, IService<Address> serviceAddress, IService<AppUser> serviceAppUser, IService<Order> serviceOrder)
        {
            _service = service;
            _serviceAddress = serviceAddress;
            _serviceAppUser = serviceAppUser;
            _serviceOrder = serviceOrder;
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

        [Authorize,HttpPost]
        public async Task<IActionResult> CheckOut(string CardNumber,string CardMonth,string CardYear,string CVV,
            string DeliveryAddress, string BillingAddress)
        {
            var cart = GetCart();
            var appUser = await _serviceAppUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
            if (appUser == null)
            {
                return RedirectToAction("SıgnIn", "Account");
            }
            var addresses = await _serviceAddress.GetAllAsync(a => a.AppUserId == appUser.Id && a.IsActive);
            var model = new CheckOutViewModel()
            {
                CartProducts = cart.CartLines,
                TotalPrice = cart.TotalPrice(),
                Addresses = addresses
            };

            if (string.IsNullOrWhiteSpace(CardNumber) || string.IsNullOrWhiteSpace(CardYear) || string.IsNullOrWhiteSpace(CardMonth) ||
                string.IsNullOrWhiteSpace(CVV) || string.IsNullOrWhiteSpace(DeliveryAddress) || string.IsNullOrWhiteSpace(BillingAddress))
            {
                return View(model);
            }
            var teslimatAdresi = addresses.FirstOrDefault(a => a.AdressGuid.ToString() == DeliveryAddress);
            var faturaAdresi = addresses.FirstOrDefault(a => a.AdressGuid.ToString() == BillingAddress);

            //ödeme çekme işlemi
            var siparis= new Order()
            {
                AppUserId=appUser.Id,
                BillingAddress = $"{faturaAdresi.OpenAddress} {faturaAdresi.District} {faturaAdresi.City}" ,    //BillingAddress,
                DeliveryAddress = $"{teslimatAdresi.OpenAddress} {teslimatAdresi.District} {teslimatAdresi.City}" ,    //DeliveryAddress,
                CustomerId =appUser.UserGuid.ToString(),
                OrderDate=DateTime.Now,
                TotalPrice=cart.TotalPrice(),
                OrderNumber=Guid.NewGuid().ToString(),
                OrderState=0,
                OrderLines = []
            };

            foreach (var item in cart.CartLines)
            {
                siparis.OrderLines.Add(new OrderLine()
                {
                    ProductId=item.Product.Id,
                    Quantity=item.Quantity,
                    OrderId=siparis.Id,
                    UnitPrice=item.Product.Price,
                });
            }
            try
            {
                await _serviceOrder.AddAsync(siparis);
                var sonuc = await _serviceOrder.SaveChangesAsync();
                if (sonuc>0)
                {
                    HttpContext.Session.Remove("Cart");
                    return RedirectToAction("Thanks");
                }
            }
            catch (Exception)
            {

               TempData["Message"] = "Siparişiniz alınırken bir hata oluştu.Lütfen tekrar deneyiniz.";
            }

            return View(model);
        }

        public IActionResult Thanks()
        {
            return View();
        }


        private CartService GetCart()
        {
            return HttpContext.Session.GetJson<CartService>("Cart") ?? new CartService();
        }
        


    }
}
