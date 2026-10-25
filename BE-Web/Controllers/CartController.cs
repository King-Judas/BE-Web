using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BE_Web.Models;

namespace BE_Web.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public List<CartItem> GetCart()
        {
            List<CartItem> cart = Session["Cart"] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }
            return cart;
        }

        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductID == id);

            if (item == null)
            {
                var product = db.Products.Find(id);
                if (product != null)
                {
                    cart.Add(new CartItem
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        ProductImage = product.ProductImage,
                        ProductPrice = product.ProductPrice,
                        Quantity = quantity
                    });
                }
            }
            else
            {
                item.Quantity += quantity;
            }

            return RedirectToAction("Index");
        }

        public ActionResult Index()
        {
            var cart = GetCart();
            ViewBag.Total = cart.Sum(item => item.TotalPrice);
            return View(cart);
        }

        public ActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductID == id);
            if (item != null)
            {
                cart.Remove(item);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Checkout()
        {
            var cart = GetCart();
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index");
            }
            ViewBag.Total = cart.Sum(item => item.TotalPrice);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(string shippingAddress)
        {
            var cart = GetCart();
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index");
            }

            Order order = new Order
            {
                CustomerID = 1, 
                OrderDate = System.DateTime.Now,
                TotalAmount = cart.Sum(item => item.TotalPrice),
                PaymentStatus = "Chưa thanh toán",
                AddressDelivery = shippingAddress
            };

            db.Orders.Add(order);
            db.SaveChanges(); 

            foreach (var item in cart)
            {
                OrderDetail detail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.ProductPrice
                };
                db.OrderDetails.Add(detail);
            }
            db.SaveChanges();

            Session["Cart"] = null;

            return RedirectToAction("OrderSuccess");
        }

        public ActionResult OrderSuccess()
        {
            return View();
        }
    }
}