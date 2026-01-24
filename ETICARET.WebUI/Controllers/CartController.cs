using ETICARET.Business.Abstract;
using ETICARET.WebUI.Identity;
using ETICARET.WebUI.Models;
using Iyzipay.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETICARET.WebUI.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ICartService cartService, IProductService productService, IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _productService = productService;
            _orderService = orderService;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var cart = _cartService.GetCartByUserId(_userManager.GetUserId(User));

            return View(
                    new CartModel()
                    {
                        CartId = cart.Id,
                        CartItems = cart.CartItems.Select(ci => new CartItemModel()
                        {
                            CartItemId = ci.Id,
                            ProductId = ci.Product.Id,
                            Name = ci.Product.Name,
                            Price = ci.Product.Price,
                            ImageUrl = ci.Product.Images[0].ImageUrl,
                            Quantity = ci.Quantity
                        }).ToList()
                    }
                );
        }

        public IActionResult AddToCart(int productId, int quantity)
        {
            _cartService.AddToCart(_userManager.GetUserId(User), productId, quantity);
            return RedirectToAction("Index");
        }

        public IActionResult DeleteFromCart(int productId)
        {
            _cartService.DeleteFromCart(_userManager.GetUserId(User), productId);
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var cart = _cartService.GetCartByUserId(_userManager.GetUserId(User));

            OrderModel orderModel = new OrderModel()
            {
                CartModel = new CartModel()
                {
                    CartId = cart.Id,
                    CartItems = cart.CartItems.Select(ci => new CartItemModel()
                    {
                        CartItemId = ci.Id,
                        ProductId = ci.Product.Id,
                        Name = ci.Product.Name,
                        Price = ci.Product.Price,
                        ImageUrl = ci.Product.Images[0].ImageUrl,
                        Quantity = ci.Quantity
                    }).ToList()
                }
            };

            return View(orderModel);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(OrderModel model, string paymentMethod)
        {
           return View(model);
        }




    }
}
