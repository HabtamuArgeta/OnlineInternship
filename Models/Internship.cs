using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternshipDotCom.Models
{
    public class Internship
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Provide Name of the internship")]
        public string InternshipName { get; set; }

        public string InternshipType { get; set; }

        public DateTime PostedAt { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Enter number of Applicant needed ")]
        [Range(1, int.MaxValue, ErrorMessage = "Not with in the valid Applicant range")]
        public int NumberOfApplicant { get; set; }


        [Required(ErrorMessage = "Please Enter Address")]
        public string Address { get; set; }
        

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500)]
        public  string Description { get; set; }

        [RegularExpression("^[1-9]*$", ErrorMessage = "Select organization")]
        public int OrganizationId { get; set; } 
        public Organization Organization { get; set; }


        [Display(Name = "Image File")]
        [NotMapped]
        public IFormFile? Image { get; set; }
        public string? ImagePath { get; set; }


        public ICollection<ApplicantInternship> ApplicantInternship { get; set; } 


    }
}
