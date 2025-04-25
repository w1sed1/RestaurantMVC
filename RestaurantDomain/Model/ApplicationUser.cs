using Microsoft.AspNetCore.Identity;

namespace RestaurantDomain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}