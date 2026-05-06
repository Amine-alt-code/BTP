using Microsoft.AspNetCore.Identity;

namespace LocationBTP.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Nom { get; set; }
        public string Prenom { get; set; }
    }
}