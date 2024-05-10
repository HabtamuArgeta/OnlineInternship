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
                .Where(ai => ai.DepartmentId == coordinatorDepartmentId && ai.UniversityId == coordinatorUniversityId)
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


    }
}

