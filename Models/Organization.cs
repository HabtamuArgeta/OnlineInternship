using Microsoft.AspNetCore.Identity;

namespace InternshipDotCom.Models
{
    public class Organization
    {
        public int Id { get; set; }

        public string OrganizationName { get; set; }

        public string Address { get; set; }

        public string Description { get; set; }

        // Foreign key to represent the ID of the user who created the organization
        public string ApplicationUserId { get; set; }

        // Navigation property to represent the user who created the organization
        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<Internship> Internships { get; set; }
    }
}
