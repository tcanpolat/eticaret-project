using System.ComponentModel.DataAnnotations;

namespace ETICARET.API.Models
{
    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string? PhoneNumber { get; set; }
    }
}
