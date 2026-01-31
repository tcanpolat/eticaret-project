using Microsoft.AspNetCore.Identity;

namespace ETICARET.API.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
