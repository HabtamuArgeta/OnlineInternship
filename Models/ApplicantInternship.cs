﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternshipDotCom.Models
{
    public class ApplicantInternship
    {
        public int Id { get; set; }   
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Department { get; set; }
        public string? University { get; set; }
        public string? Year { get; set; }

        public string? CoverLetter { get; set; }

        public bool IsCalledForInterview { get; set; }

        [Display(Name = "Resume / Cv")]
        [NotMapped]
        public IFormFile? Resume { get; set; }
        public string? ResumePath { get; set; }
        public int InternshipId { get; set; }
        public Internship Internship { get; set; }
        public bool IsSaved { get; set; }
        public bool IsApplied { get; set; }
    }
}

