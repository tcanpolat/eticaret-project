using ETICARET.API.Identity;
using ETICARET.Business.Abstract;
using ETICARET.Business.Concrete;
using ETICARET.DataAccess.Abstract;
using ETICARET.DataAccess.Concrete.EfCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });


builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"))
);

// Identity ayarlarý
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
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
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationIdentityDbContext>()
.AddDefaultTokenProviders();


/*
 *** JWT (JSON WEB TOKEN)
 * Kullanýcýnýn sisteme giriþ yaptýðýnda aldýðý bir özel kimlik kartý gibidir.
 * Her Api isteðinde bu tokený kullanarak kimliðiniz kanýtlanýr ve istek atabilirsiniz
 */

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

// Kimlik Doðrulama Servisi
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Tokený kim üretti
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],

        // Token kim için üretildi
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],

        // Token için bir life time var mý
        ValidateLifetime = true,

        // Signature (Ýmza)
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

        // Token süresi doldu tolerans olsun mu
        ClockSkew = TimeSpan.Zero
    };

    // Debug için
    options.Events = new JwtBearerEvents
    {
        // OnAuthenticationFailed olayý, kimlik doðrulama baþarýsýz olduðunda tetiklenir
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        // OnTokenValidated olayý, token doðrulandýktan sonra tetiklenir
        OnTokenValidated = context =>
        {
            Console.WriteLine("OnTokenValidated: " + context.SecurityToken);
            return Task.CompletedTask;
        },
        // OnChallenge olayý, kimlik doðrulama baþarýsýz olduðunda tetiklenir
        OnChallenge = context =>
        {
            Console.WriteLine("OnChallenge: " + context.Error);
            return Task.CompletedTask;
        }
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

// Swagger Yapýlandýrmasý (Api dökümantasyonu ve Test arayüzü)
// Test ortamýnda swagger ayarlarý
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ETICARET.API",
        Version = "v1",
        Description = "ETICARET API Dökümantasyonu"
    });

    // Swagger JWT Authentication Ayarlarý
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization : Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header, // Parameter'in header'da olduðunu belirtir
        Type = SecuritySchemeType.ApiKey, // ApiKey türünde olduðunu belirtir
        Scheme = "Bearer" // Bearer þemasýný kullanýr
    });

    // Endpoint'lerde güvenlik gereksinimi ekleme
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            Array.Empty<string>() // Tüm scopelar için boþ array döndürür
        }
    });

});

// Cors ayarlarý (api'ye nereden eriþilebileceðini ayarladýðýmýz bölüm)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
