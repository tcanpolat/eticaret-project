namespace ETICARET.API.Models
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public DateTime Expiration {  get; set; }
    }
}
