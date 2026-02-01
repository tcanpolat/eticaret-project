using ETICARET.API.Identity;
using ETICARET.API.Models;
using ETICARET.Business.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ETICARET.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ICartService _cartService;

        public LoginController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ICartService cartService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _cartService = cartService;
        }

        // login endpointi
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.ErrorResponse("Geçersiz giriş bilgileri", errors));
            }
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Kullanıcı bulunamadı"));
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);

            if (result.Succeeded)
            {
                var token = await GenerateJwtToken(user);

                var response = new LoginResponse
                {
                    Token = token,
                    Email = user.Email,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationInMinutes"])) // Token geçerlilik süresi
                };

                return Ok(ApiResponse<LoginResponse>.SuccessResponse(response,"Giriş Başarılı"));
            }

            if (result.IsLockedOut)
            {
                return StatusCode(StatusCodes.Status423Locked, ApiResponse<object>.ErrorResponse("Hesabınız belirli bir süreliğine kilitlendi. Lütfen daha sonra tekrar deneyiniz."));
            }

            return Unauthorized(ApiResponse<object>.ErrorResponse("Email veya parola hatalı"));

        }

        // Yeni kullanıcı kaydı
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.ErrorResponse("Geçersiz kayıt bilgileri",errors));
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null) 
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Bu email adresi zaten mevcut"));
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true // email doğrulamasını atla
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded) 
            {
                _cartService.InitialCart(user.Id);

                var token = await GenerateJwtToken(user);

                var response = new LoginResponse
                {
                    Token = token,
                    Email = user.Email,
                    UserName = user.FullName,
                    FullName = user.FullName,
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationInMinutes"])) // Token geçerlilik süresi
                };

                return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Kayıt Başarılı"));
                
            }

            var errorList = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(ApiResponse<object>.ErrorResponse("Kayıt Başarısız", errorList));

        }


        // JWT token oluşturma metodu
        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            // JWT token nedir?
            // JSON Web Token (JWT), iki taraf arasında güvenli bilgi alışverişi için kullanılan kompakt, URL-safe bir token formatıdır.
            // Jwt token 3 ana bölümden oluşur: Header, Payload ve Signature.
            // 1.Header: Token türünü (JWT) ve kullanılan imzalama algoritmasını belirtir.
            // 2.Payload: Kullanıcı bilgileri ve token ile ilgili diğer verileri içerir.
            // 3.Signature: Token'ın bütünlüğünü ve doğruluğunu sağlamak için kullanılır.
            // Örnek bir JWT token yapısı: eyJhbGciOi4533Fhrd323ge.43ff2f4hlhişsdfsfvs.eyJhbGciOi5464vsa3gNmÖ
            //                             [Header]                 [Payload]         [Signature]

            // 1. adım jwt ayarları alındı
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]; // Gizli anahtar kimseyle paylaşılmaz ve bilinmemelidir.

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT Secret key yapılandırması bulunmuyor");
            }

            // 2. adım: İmza ve şifreleme algoritması oluşturuldu
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creadentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. adım: Token içeriği (Kullanıcın rolü,email,bilgileri) oluşturuldu
            // rolü al
            var roles = await _userManager.GetRolesAsync(user);

            // 4. adım: Claims talepler listesi oluşturuldu

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "" ),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                // .NET Standart claim tipleri
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),

                // özel claimler
                new Claim("fullName", user.FullName ?? "")
            };
            // rollerini claims listesine ekle

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 5. adım: Token oluşturma
            var token  = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"], // Token'ı kim üretti
                audience: jwtSettings["Audience"], // Token'ı kim kullanacak
                claims: claims, // Token içeriğindeki özel bilgiler
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationInMinutes"] ?? "60")), // Token'ın geçerlilik süresi
                signingCredentials: creadentials // İmza ve şifreleme bilgileri
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}
