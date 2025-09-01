using ETicaret.Core.Entities;
using ETicaret.Service.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ETicaret.Service.Concrete
{
    public class CartService : ICartService
    {
        public List<CartLine> CartLines = new();
        public void AddProduct(Product product, int quantity)
        {
            var urun=CartLines.FirstOrDefault(p =>p.Product.Id==product.Id);
            if (urun != null)
            {
                urun.Quantity += quantity;
            }
            else 
            {
                CartLines.Add(new CartLine
                {
                    Product = product,
                    Quantity = quantity
                });
            }
        }

        public void ClearAll()
        {
            CartLines.Clear();
        }

        public void RemoveProduct(Product product)
        {
            CartLines.RemoveAll(p=>p.Product.Id==product.Id);
        }

        public decimal TotalPrice()
        {
            return CartLines.Sum(c=>c.Product.Price * c.Quantity);
        }

        public void UpdateProduct(Product product, int quantity)
        {
            var urun = CartLines.FirstOrDefault(p => p.Product.Id == product.Id);
            if (urun != null)
            {
                urun.Quantity += quantity;

                // Eğer miktar 0 veya altına düşerse ürünü kaldır
                if (urun.Quantity <= 0)
                {
                    CartLines.Remove(urun);
                }
            }
            else if (quantity > 0)
            {
                CartLines.Add(new CartLine
                {
                    Product = product,
                    Quantity = quantity
                });
            }
        }

    }
}
