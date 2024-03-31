using Microsoft.AspNetCore.Identity;

namespace InternshipDotCom.Models
{
    public class ApplicationUser : IdentityUser 
    {
        public string FristName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string RegisteredAs { get; set; } = string.Empty;

        public bool Approved { get; set; } = false;
    }
}
