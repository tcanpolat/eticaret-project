using ETICARET.Business.Abstract;
using ETICARET.Entities;
using ETICARET.WebUI.EmailService;
using ETICARET.WebUI.Extensions;
using ETICARET.WebUI.Identity;
using ETICARET.WebUI.Models;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using OrderItem = ETICARET.Entities.OrderItem;

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
            ModelState.Remove("CartModel");

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var cart = _cartService.GetCartByUserId(userId);

                model.CartModel = new CartModel()
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
                };

                if (paymentMethod == "credit")
                {
                    // Ödeme işlemleri ve Sipariş oluşturma burada gerçekleştirilecek
                    var payment = PaymentProccess(model);

                    if (payment.Result.Status == "success")
                    {
                        SaveOrder(model, payment, userId);
                        ClearCart(cart.Id.ToString());
                        string mailBody =
                        "<div style='width:100%; background-color:#f4f6f8; padding:30px 0; font-family:Arial, Helvetica, sans-serif;'>" +
                        "  <div style='max-width:600px; margin:0 auto; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.05);'>" +

                        "    <div style='background-color:#4f46e5; padding:20px; text-align:center;'>" +
                        "      <h1 style='margin:0; font-size:22px; color:#ffffff;'>Sipariş Onayı</h1>" +
                        "    </div>" +

                        "    <div style='padding:30px; color:#333333; font-size:14px; line-height:1.6;'>" +
                        "      <p style='margin-top:0;'>Sayın <strong>" + model.FirstName + " " + model.LastName + "</strong>,</p>" +

                        "      <p>Siparişiniz başarıyla alınmıştır. Sipariş detaylarınız aşağıda yer almaktadır:</p>" +

                        "      <div style='background-color:#f9fafb; border:1px solid #e5e7eb; border-radius:6px; padding:15px; margin:20px 0;'>" +
                        "        <p style='margin:0 0 8px 0;'>" +
                        "          <strong>Sipariş Numaranız:</strong><br/>" +
                        "          <span style='color:#4f46e5;'>" + payment.Result.ConversationId + "</span>" +
                        "        </p>" +
                        "        <p style='margin:0;'>" +
                        "          <strong>Toplam Tutar:</strong><br/>" +
                        "          <span style='font-size:16px; font-weight:bold;'>" + payment.Result.PaidPrice + " TL</span>" +
                        "        </p>" +
                        "      </div>" +

                        "      <p>Bizi tercih ettiğiniz için teşekkür ederiz.</p>" +

                        "      <p style='margin-bottom:0;'>Saygılarımızla,<br/><strong>Üçüncübinyıl Akademi</strong></p>" +
                        "    </div>" +

                        "    <div style='background-color:#f3f4f6; padding:15px; text-align:center; font-size:12px; color:#6b7280;'>" +
                        "      Bu e-posta bilgilendirme amaçlıdır." +
                        "    </div>" +

                        "  </div>" +
                        "</div>";

                        MailHelper.SendEmail(mailBody, model.Email, "Sipariş Onayı",true);
                        TempData.Put("message", new ResultModel()
                        {
                            Title="Sipariş Tamamlandı",
                            Message="Siparişiniz başarıyla tamamlandı. Teşekkür ederiz.",
                            Css="success"
                        });
                    }
                    else
                    {
                        TempData.Put("message", new ResultModel()
                        {
                            Title="Hata Oluştu",
                            Message="Ödeme işlemi sırasında bir hata oluştu: " + payment.Result.ErrorMessage,
                            Css="danger"
                        });
                        return View(model);
                    }
                }
                else
                {
                    // Eft için Sipariş oluşturma

                    SaveOrder(model, userId);
                    ClearCart(cart.Id.ToString());
                    string mailBody =
                    "<div style='width:100%; background-color:#f4f6f8; padding:30px 0; font-family:Arial, Helvetica, sans-serif;'>" +
                    "  <div style='max-width:600px; margin:0 auto; background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.05);'>" +

                    "    <div style='background-color:#4f46e5; padding:20px; text-align:center;'>" +
                    "      <h1 style='margin:0; font-size:22px; color:#ffffff;'>Sipariş Onayı</h1>" +
                    "    </div>" +

                    "    <div style='padding:30px; color:#333333; font-size:14px; line-height:1.6;'>" +
                    "      <p style='margin-top:0;'>Sayın <strong>" + model.FirstName + " " + model.LastName + "</strong>,</p>" +

                    "      <p>Siparişiniz başarıyla alınmıştır. Sipariş detaylarınız aşağıda yer almaktadır:</p>" +

                    "      <p>Bizi tercih ettiğiniz için teşekkür ederiz.</p>" +

                    "      <p style='margin-bottom:0;'>Saygılarımızla,<br/><strong>Üçüncübinyıl Akademi</strong></p>" +
                    "    </div>" +

                    "    <div style='background-color:#f3f4f6; padding:15px; text-align:center; font-size:12px; color:#6b7280;'>" +
                    "      Bu e-posta bilgilendirme amaçlıdır." +
                    "    </div>" +

                    "  </div>" +
                    "</div>";

                    MailHelper.SendEmail(mailBody, model.Email, "Sipariş Onayı", true);

                    TempData.Put("message", new ResultModel()
                    {
                        Title="Sipariş Tamamlandı",
                        Message="Siparişiniz başarıyla tamamlandı. Teşekkür ederiz.",
                        Css="success"
                    });
                }
            }
            else
            {
                return View(model);

            }

            return RedirectToAction("Index", "Home");
        }

      
        private async Task<Payment> PaymentProccess(OrderModel model)
        {
            Options options = new Options()
            {
                BaseUrl = "https://sandbox-api.iyzipay.com",
                ApiKey = "sandbox-cNnJEaoyNt0sCREL4nOq8PajTLQwWeXz",
                SecretKey = "sandbox-cmJxJfaGlVarqNV3c5ZQcMTwVNh8qswx"
            };

            string externalIpString = new WebClient().DownloadString("https://www.icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            var externalIp = IPAddress.Parse(externalIpString);

            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = Guid.NewGuid().ToString();
            request.Price = model.CartModel.TotalPrice().ToString().Split(',')[0];
            request.PaidPrice = model.CartModel.TotalPrice().ToString().Split(',')[0];
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.BasketId = model.CartModel.CartId.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            request.PaymentChannel = PaymentChannel.WEB.ToString();

            PaymentCard paymentCard = new PaymentCard()
            {
                CardHolderName = model.CardName,
                CardNumber = model.CardNumber,
                ExpireYear = model.ExprationYear,
                ExpireMonth = model.ExprationMonth,
                Cvc = model.CVV
            };

            request.PaymentCard = paymentCard;

            Buyer buyer = new Buyer()
            {
                Id = _userManager.GetUserId(User),
                Name = model.FirstName,
                Surname = model.LastName,
                GsmNumber = model.Phone,
                Email = model.Email,
                IdentityNumber = "11111111111",
                RegistrationAddress = model.Address,
                Ip = externalIp.ToString(),
                City = model.City,
                Country = "TURKEY",
                ZipCode = "34000"
            };

            request.Buyer = buyer;

            Address address = new Address()
            {
                ContactName = model.FirstName + " " + model.LastName,
                City = model.City,
                Country = "TURKEY",
                Description = model.Address,
                ZipCode = "34000"
            };

            request.ShippingAddress = address;
            request.BillingAddress = address;

            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem basketItem;

            foreach (var cartItem in model.CartModel.CartItems)
            {
                basketItem = new BasketItem()
                {
                    Id = cartItem.ProductId.ToString(),
                    Name = cartItem.Name,
                    Category1 = _productService.GetProductDetail(cartItem.ProductId).ProductCategories.FirstOrDefault().CategoryId.ToString(),
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = (cartItem.Price * cartItem.Quantity).ToString().Split(",")[0]
                };

                basketItems.Add(basketItem);
            }

            request.BasketItems = basketItems;

            Payment payment = await Payment.Create(request,options);

            return payment;
        }

        private void ClearCart(string id)
        {
            _cartService.ClearCart(id);
        }

        // Kredi kartı ile sipariş kaydetme metodu
        private void SaveOrder(OrderModel model, Task<Payment> payment, string userId)
        {
            Order order = new Order()
            {
                OrderNumber = Guid.NewGuid().ToString(),
                OrderState = EnumOrderState.completed,
                PaymentTypes = EnumPaymentType.CreditCard,
                PaymentToken = Guid.NewGuid().ToString(),
                ConversationId = payment.Result.ConversationId,
                PaymentId = payment.Result.PaymentId,
                OrderNote = model.OrderNote,
                OrderDate = DateTime.Now,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                City = model.City,
                Phone = model.Phone,
                Email = model.Email,
                UserId = userId,
            };

            foreach (var cartItem in model.CartModel.CartItems)
            {
                var orderItem = new ETICARET.Entities.OrderItem()
                {
                    Price = cartItem.Price,
                    Quantity = cartItem.Quantity,
                    ProductId = cartItem.ProductId
                };

                order.OrderItems.Add(orderItem);
            }

            _orderService.Create(order);

        }
        // Eft ile sipariş kaydetme metodu
        private void SaveOrder(OrderModel model, string userId)
        {
            Order order = new Order()
            {
                OrderNumber = Guid.NewGuid().ToString(),
                OrderState = EnumOrderState.completed,
                PaymentTypes = EnumPaymentType.Eft,
                PaymentToken = Guid.NewGuid().ToString(),
                ConversationId = Guid.NewGuid().ToString(),
                PaymentId = Guid.NewGuid().ToString(),
                OrderNote = model.OrderNote,
                OrderDate = DateTime.Now,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                City = model.City,
                Phone = model.Phone,
                Email = model.Email,
                UserId = userId,
            };

            foreach (var cartItem in model.CartModel.CartItems)
            {
                var orderItem = new ETICARET.Entities.OrderItem()
                {
                    Price = cartItem.Price,
                    Quantity = cartItem.Quantity,
                    ProductId = cartItem.ProductId
                };

                order.OrderItems.Add(orderItem);
            }

            _orderService.Create(order);
        }

        public IActionResult GetOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = _orderService.GetOrders(userId);

            var orderListModel = new List<OrderListModel>();

            OrderListModel orderModel;

            foreach (var order in orders)
            {
                orderModel = new OrderListModel();
                orderModel.OrderId = order.Id;
                orderModel.OrderNumber = order.OrderNumber;
                orderModel.OrderDate = order.OrderDate;
                orderModel.OrderState = order.OrderState;
                orderModel.PaymentTypes = order.PaymentTypes;
                orderModel.FirstName = order.FirstName;
                orderModel.LastName = order.LastName;
                orderModel.Address = order.Address;
                orderModel.City = order.City;
                orderModel.Phone = order.Phone;
                orderModel.Email = order.Email;
                orderModel.OrderItems = order.OrderItems.Select(oi => new OrderItemModel()
                {
                    OrderItemId = oi.Id,
                    Name = oi.Product.Name,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    ImageUrl = oi.Product.Images[0].ImageUrl
                }).ToList();

                orderListModel.Add(orderModel);
            }
            orderListModel = orderListModel.OrderByDescending(o => o.OrderDate).ToList();

            return View(orderListModel);
        }
    }
}
