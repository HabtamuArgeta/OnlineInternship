namespace InternshipDotCom.Models
{
    public class Application
    {
        public int Id { get; set; }

        public int ApplicantId { get; set; } // Foreign key
        public Applicant Applicant { get; set; } // Navigation property

        public int InternshipId { get; set; } // Foreign key
        public Internship Internship { get; set; } // Navigation property
    }
}
