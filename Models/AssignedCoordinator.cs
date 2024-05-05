using System.ComponentModel.DataAnnotations;

namespace InternshipDotCom.Models
{
    public class AssignedCoordinator
    {
        public int Id { get; set; }


        
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }


        [RegularExpression("^[1-9]*$", ErrorMessage = "Select University")]
        public int UniversityId { get; set; }

        public University university { set; get; }


        [RegularExpression("^[1-9]*$", ErrorMessage = "Select Department")]
        public int DepartmentId { get; set; }

        public Department department { set; get; }

    }
}
