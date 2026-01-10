namespace ETICARET.WebUI.Models
{
    public class AdminUserModel
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
