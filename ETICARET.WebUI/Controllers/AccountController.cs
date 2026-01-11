using ETICARET.Business.Abstract;
using ETICARET.WebUI.EmailService;
using ETICARET.WebUI.Extensions;
using ETICARET.WebUI.Identity;
using ETICARET.WebUI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETICARET.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ICartService _cartService;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,ICartService cartService)
        {
            _cartService = cartService;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Generate email confirmation token
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", 
                    new { 
                        userId = user.Id, 
                        token = code
                    }
                );

                string siteUrl = "https://localhost:7120";
                string activeUrl = $"{siteUrl}{callbackUrl}"; // https://localhost:7120/Account/ConfirmEmail?userId=1&token=ad34adj38ryxx2342

                string body = $"Hesabınızı aktifleştirmek için <a href='{activeUrl}'>tıklayınız</a>";

                // Email Service ile email gönderme işlemi yapılacak
                MailHelper.SendEmail(body, user.Email, "Hesabınızı Aktifleştirin");

                return RedirectToAction("Login", "Account");

            }

            TempData.Put("message", new ResultModel()
            {
                Title="Kayıt Hatası",
                Message="Kullanıcı oluşturulurken bir hata oluştu.Lütfen tekrar deneyiniz.",
                Css="danger"
            });

            return View(model);
        }
    }
}
