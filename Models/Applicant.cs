namespace InternshipDotCom.Models
{
    public class Applicant
    {

        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Department { get; set; }

        
        public string University { get; set; }

        public string year { get; set; }

       
        public ICollection<ApplicantInternship> ApplicantInternship { get; set; } 

      

    }
}