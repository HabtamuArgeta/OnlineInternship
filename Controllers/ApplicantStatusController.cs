using System.Linq;
using InternshipDotCom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using InternshipDotCom.Servieces;

namespace InternshipDotCom.Controllers
{
    [Authorize(Roles = "InternshipCordinator")]
    public class ApplicantStatusController : Controller
    {
        private readonly ApplicationDbContext _context;
        

        public ApplicantStatusController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            
        }

        public IActionResult Index()
        {

            var AllYear = _context.YearOfStudy.ToList();
            ViewBag.Year = AllYear;
            // Get the current user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Find the coordinator's department and university
            var coordinator = _context.AssignedCoordinator
                .Include(ac => ac.ApplicationUser)
                .Include(ac => ac.university)
                .Include(ac => ac.department)
                .FirstOrDefault(ac => ac.ApplicationUserId == userId);

            if (coordinator == null)
            {
                return View("NotAssigned");
            }

            // Get the coordinator's department and university IDs
            var coordinatorDepartmentId = coordinator.DepartmentId;
            var coordinatorUniversityId = coordinator.UniversityId;

            

            // Query applicants whose department and university match those of the coordinator
            var matchingApplicants = _context.ApplicantInternship
                .Include(ai => ai.ApplicationUser)
                .Include(ai => ai.university)
                .Include(ai => ai.department)
                .Where(ai => ai.DepartmentId == coordinatorDepartmentId && ai.UniversityId == coordinatorUniversityId && ai.IsApplied)
                .ToList();

            return View(matchingApplicants);
        }

        public async Task<IActionResult> GetApplicantDetails(int applicantId)
        {
            var applicant = await _context.ApplicantInternship
                .Include(ai => ai.ApplicationUser)
                .Include(ai => ai.university)
                .Include(ai => ai.department)
                .Include(ai => ai.YearOfStudy)
                .FirstOrDefaultAsync(ai => ai.Id == applicantId);

            if (applicant == null)
            {
                return NotFound();
            }

            return PartialView("_ApplicantDetails", applicant);
        }

        public async Task<IActionResult> Filter(int? YearId)
        {


            var AllYear = _context.YearOfStudy.ToList();
            ViewBag.Year = AllYear;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Find the coordinator's department and university
            var coordinator = _context.AssignedCoordinator
                .Include(ac => ac.ApplicationUser)
                .Include(ac => ac.university)
                .Include(ac => ac.department)
                .FirstOrDefault(ac => ac.ApplicationUserId == userId);

            if (coordinator == null)
            {
                return View("NotAssigned");
            }

            // Get the coordinator's department and university IDs
            var coordinatorDepartmentId = coordinator.DepartmentId;
            var coordinatorUniversityId = coordinator.UniversityId;



            // Query applicants whose department and university match those of the coordinator
          
            var ApplicantQuery = _context.ApplicantInternship
                .Include(i => i.YearOfStudy)
                .Include(i => i.ApplicationUser)
                .AsQueryable();


            if (YearId.HasValue)
            {
                ApplicantQuery = ApplicantQuery.Where(i => i.YearOfStudyId == YearId && i.DepartmentId == coordinatorDepartmentId && i.UniversityId == coordinatorUniversityId && i.IsApplied);
            }


            var filteredApplicant = await ApplicantQuery.ToListAsync();


            return PartialView("_ApplicantTable", filteredApplicant);

        }

        public IActionResult Search(string SearchedUsername)
        {
            var AllYear = _context.YearOfStudy.ToList();
            ViewBag.Year = AllYear;
            // Get the current user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Find the coordinator's department and university
            var coordinator = _context.AssignedCoordinator
                .Include(ac => ac.ApplicationUser)
                .Include(ac => ac.university)
                .Include(ac => ac.department)
                .FirstOrDefault(ac => ac.ApplicationUserId == userId);

            if (coordinator == null)
            {
                return View("NotAssigned");
            }

            // Get the coordinator's department and university IDs
            var coordinatorDepartmentId = coordinator.DepartmentId;
            var coordinatorUniversityId = coordinator.UniversityId;



            // Query applicants whose department and university match those of the coordinator
            var matchingApplicants = _context.ApplicantInternship
                .Include(ai => ai.ApplicationUser)
                .Include(ai => ai.university)
                .Include(ai => ai.department)
                .Where(ai => ai.DepartmentId == coordinatorDepartmentId && ai.UniversityId == coordinatorUniversityId && ai.ApplicationUser.UserName == SearchedUsername && ai.IsApplied)
                .ToList();

            return PartialView("_ApplicantTable", matchingApplicants);


        }

        }
}

