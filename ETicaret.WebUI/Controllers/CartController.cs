using ETicaret.Core.Entities;
using ETicaret.Service.Abstract;
using ETicaret.Service.Concrete;
using ETicaret.WebUI.ExtensionMethods;
using ETicaret.WebUI.Models;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ETicaret.WebUI.Controllers
{
    public class CartController : Controller
    {
        private readonly IService<Product> _service;
        private readonly IService<Core.Entities.Address> _serviceAddress;
        private readonly IService<AppUser> _serviceAppUser;
        private readonly IService<Order> _serviceOrder;
        private readonly IConfiguration _configuration;

        public CartController(IService<Product> service, IService<Core.Entities.Address> serviceAddress, IService<AppUser> serviceAppUser, IService<Order> serviceOrder, IConfiguration configuration)
        {
            _service = service;
            _serviceAddress = serviceAddress;
            _serviceAppUser = serviceAppUser;
            _serviceOrder = serviceOrder;
            _configuration = configuration;
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
            #region OdemeIslemi
            Options options = new Options();
            options.ApiKey = _configuration["IyzicOptions:ApiKey"];
            options.SecretKey = _configuration["IyzicOptions:SecretKey"];
            options.BaseUrl = _configuration["IyzicOptions:BaseUrl"];  //"https://sandbox-api.iyzipay.com";

            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = "123456789";
            request.Price = "1";
            request.PaidPrice = "1.2";
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.BasketId = "B67832";
            request.PaymentChannel = PaymentChannel.WEB.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();

            PaymentCard paymentCard = new PaymentCard();
            paymentCard.CardHolderName = "John Doe";
            paymentCard.CardNumber = "5528790000000008";
            paymentCard.ExpireMonth = "12";
            paymentCard.ExpireYear = "2030";
            paymentCard.Cvc = "123";
            paymentCard.RegisterCard = 0;
            request.PaymentCard = paymentCard;

            Buyer buyer = new Buyer();
            buyer.Id = "BY789";
            buyer.Name = "John";
            buyer.Surname = "Doe";
            buyer.GsmNumber = "+905350000000";
            buyer.Email = "email@email.com";
            buyer.IdentityNumber = "74300864791";
            buyer.LastLoginDate = "2015-10-05 12:43:35";
            buyer.RegistrationDate = "2013-04-21 15:12:09";
            buyer.RegistrationAddress = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            buyer.Ip = "85.34.78.112";
            buyer.City = "Istanbul";
            buyer.Country = "Turkey";
            buyer.ZipCode = "34732";
            request.Buyer = buyer;

            var shippingAddress = new Iyzipay.Model.Address();
            shippingAddress.ContactName = "Jane Doe";
            shippingAddress.City = "Istanbul";
            shippingAddress.Country = "Turkey";
            shippingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            shippingAddress.ZipCode = "34742";
            request.ShippingAddress = shippingAddress;

            var billingAddress = new Iyzipay.Model.Address();
            billingAddress.ContactName = "Jane Doe";
            billingAddress.City = "Istanbul";
            billingAddress.Country = "Turkey";
            billingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            billingAddress.ZipCode = "34742";
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem firstBasketItem = new BasketItem();
            firstBasketItem.Id = "BI101";
            firstBasketItem.Name = "Binocular";
            firstBasketItem.Category1 = "Collectibles";
            firstBasketItem.Category2 = "Accessories";
            firstBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
            firstBasketItem.Price = "0.3";
            basketItems.Add(firstBasketItem);

            BasketItem secondBasketItem = new BasketItem();
            secondBasketItem.Id = "BI102";
            secondBasketItem.Name = "Game code";
            secondBasketItem.Category1 = "Game";
            secondBasketItem.Category2 = "Online Game Items";
            secondBasketItem.ItemType = BasketItemType.VIRTUAL.ToString();
            secondBasketItem.Price = "0.5";
            basketItems.Add(secondBasketItem);

            BasketItem thirdBasketItem = new BasketItem();
            thirdBasketItem.Id = "BI103";
            thirdBasketItem.Name = "Usb";
            thirdBasketItem.Category1 = "Electronics";
            thirdBasketItem.Category2 = "Usb / Cable";
            thirdBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
            thirdBasketItem.Price = "0.2";
            basketItems.Add(thirdBasketItem);
            request.BasketItems = basketItems;

            Payment payment =await Payment.Create(request, options);
     
            #endregion

            try
            {
                if (payment.Status == "success")
                {
                    //sipariş oluştur
                    await _serviceOrder.AddAsync(siparis);
                    var sonuc = await _serviceOrder.SaveChangesAsync();
                    if (sonuc > 0)
                    {
                        HttpContext.Session.Remove("Cart");
                        return RedirectToAction("Thanks");
                    }
                }
                else
                {
                    TempData["Message"] = "Ödeme işlemi gerçekleştirilemedi.Lütfen kart bilgilerinizi kontrol ediniz.";
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
