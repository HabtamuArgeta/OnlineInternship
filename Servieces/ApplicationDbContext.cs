using InternshipDotCom.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace InternshipDotCom.Servieces
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var admin = new IdentityRole("admin");
            admin.NormalizedName = "admin";

            var applicant = new IdentityRole("applicant");
            applicant.NormalizedName = "applicant";

            var InternshipCordinator = new IdentityRole("InternshipCordinator");
            InternshipCordinator.NormalizedName = "InternshipCordinator";

            var organization = new IdentityRole("organization");
            organization.NormalizedName = "organization";

            var Pending = new IdentityRole("Pending");
            Pending.NormalizedName = "Pending";

            builder.Entity<IdentityRole>().HasData(admin, applicant, InternshipCordinator, organization, Pending);


            builder.Entity<YearOfStudy>().HasData(
              new YearOfStudy { Id = 1, Year = "First Year" },
              new YearOfStudy { Id = 2, Year = "Second Year" },
              new YearOfStudy { Id = 3, Year = "Third Year" },
              new YearOfStudy { Id = 4, Year = "Fourth Year" },
              new YearOfStudy { Id = 5, Year = "Fifth Year" },
              new YearOfStudy { Id = 6, Year = "Sixth Year" },
              new YearOfStudy { Id = 7, Year = "Seventh Year" }
    );

        }
        public DbSet<InternshipDotCom.Models.Internship> Internship { get; set; } = default!;
        public DbSet<InternshipDotCom.Models.Organization> Organization { get; set; } = default!;
      

        public DbSet<InternshipDotCom.Models.ApplicantInternship> ApplicantInternship { get; set; } = default!;
        public DbSet<InternshipDotCom.Models.University> University { get; set; } = default!;
        public DbSet<InternshipDotCom.Models.Department> Department { get; set; } = default!;

        public DbSet<InternshipDotCom.Models.YearOfStudy> YearOfStudy { get; set; } = default!;
        public DbSet<InternshipDotCom.Models.AssignedCoordinator> AssignedCoordinator { get; set; } = default!;




    }
}
