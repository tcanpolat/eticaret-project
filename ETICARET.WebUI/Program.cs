using ETICARET.Business.Abstract;
using ETICARET.Business.Concrete;
using ETICARET.DataAccess.Abstract;
using ETICARET.DataAccess.Concrete.EfCore;
using ETICARET.WebUI.Identity;
using ETICARET.WebUI.Middlewares;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// identtiy veritabaný baðlantýsý ve kullanýcý yönetimi
builder.Services.AddDbContext<ApplicationIdentityDbContext>(
    options =>
    {
       options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
    }
);

// identity servisleri
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
    .AddDefaultTokenProviders();
// seed identity için usermanager ve rolemanager eklenmesi
var userManager = builder.Services.BuildServiceProvider().GetService<UserManager<ApplicationUser>>();
var roleManager = builder.Services.BuildServiceProvider().GetService<RoleManager<IdentityRole>>();


// Identity ayarlarý
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
});

// Authentication Cookie ayarlarý
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/accessDenied";
    options.SlidingExpiration = true; // sliding expiration => her istek yapýldýðýnda sürenin uzatýlmasý
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.Cookie = new CookieBuilder
    {
        HttpOnly = true,
        Name = ".ETICARET.Security.Cookie",
        SameSite = SameSiteMode.Strict
    };
});


// Bussiness ve Data katmanlarý için servis ekleme
builder.Services.AddScoped<IProductDal, EfCoreProductDal>();
builder.Services.AddScoped<IProductService, ProductManager>();
builder.Services.AddScoped<ICategoryDal, EfCoreCategoryDal>();
builder.Services.AddScoped<ICategoryService, CategoryManager>();
builder.Services.AddScoped<ICartDal, EfCoreCartDal>();
builder.Services.AddScoped<ICartService, CartManager>();
builder.Services.AddScoped<ICommentDal, EfCoreCommentDal>();
builder.Services.AddScoped<ICommentService, CommentManager>();
builder.Services.AddScoped<IOrderDal, EfCoreOrderDal>();
builder.Services.AddScoped<IOrderService, OrderManager>();


builder.Services.AddMvc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


SeedDatabase.Seed();

app.UseStaticFiles();
app.CustomStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseRouting();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
        name: "products",
        pattern: "products/{category}",
        defaults: new { controller = "Shop", action = "List" }
    );

    endpoints.MapControllerRoute(
        name: "adminProducts",
        pattern: "admin/products",
        defaults: new { controller = "Admin", action = "ProductList" }
    );

    endpoints.MapControllerRoute(
        name: "adminProducts",
        pattern: "admin/products/{id}",
        defaults: new { controller = "Admin", action = "EditProduct" }
    );

    endpoints.MapControllerRoute(
        name: "adminProducts",
        pattern: "admin/category",
        defaults: new { controller = "Admin", action = "CategoryList" }
    );
    endpoints.MapControllerRoute(
       name: "adminProducts",
       pattern: "admin/categories/{id}",
       defaults: new { controller = "Admin", action = "EditCategory" }
   );

    endpoints.MapControllerRoute(
          name: "cart",
          pattern: "cart",
          defaults: new { controller = "Cart", action = "Index" }
    );
    endpoints.MapControllerRoute(
         name: "checkout",
         pattern: "checkout",
         defaults: new { controller = "Cart", action = "Checkout" }
    );

    endpoints.MapControllerRoute(
        name: "order",
        pattern: "order",
        defaults: new { controller = "Cart", action = "GetOrders" }
    );

});

SeedIdentity.Seed(userManager,roleManager,app.Configuration);

app.Run();
