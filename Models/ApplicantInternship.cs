namespace InternshipDotCom.Models
{
    public class ApplicantInternship
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public Applicant Applicant { get; set; }

        public int InternshipId { get; set; }
        public Internship Internship { get; set; }

        public bool IsSaved { get; set; }
        public bool IsApplied { get; set; }
    }
}
