using ETICARET.API.Models;
using ETICARET.Business.Abstract;
using ETICARET.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ETICARET.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // JWT ile koruma altına alınmış controller
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // Tüm ürünleri listeleme endpointi
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            try
            {
                var products = _productService.GetAll();

                if(products == null || !products.Any())
                {
                    return NotFound(ApiResponse<List<Product>>.SuccessResponse(new List<Product>(),"Ürün bulanamadı"));
                }

                return Ok(ApiResponse<List<Product>>.SuccessResponse(products,"Ürünler başarıyla getirildi."));
            }
            catch (Exception ex)
            {
                return StatusCode(500,ApiResponse<object>.ErrorResponse($"Bir Hata oluştu: {ex.Message} "));
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id) 
        {
            try
            {
                var product = _productService.GetById(id);
                if(product == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Ürün bulunamadı"));
                }
                return Ok(ApiResponse<Product>.SuccessResponse(product,"Ürün başarıyla getirildi."));
            }
            catch (Exception ex)
            {
                return StatusCode(500,ApiResponse<object>.ErrorResponse($"Bir Hata oluştu: Hata Mesajı: {ex.Message}"));
            }
        
        }
    }
}
