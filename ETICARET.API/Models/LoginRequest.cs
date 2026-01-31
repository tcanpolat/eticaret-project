using System.ComponentModel.DataAnnotations;

namespace ETICARET.API.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email alanı zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Email alanı zorunludur")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
